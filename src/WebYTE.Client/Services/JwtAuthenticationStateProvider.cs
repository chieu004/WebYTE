using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebYTE.Client.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token) || !token.Contains('.'))
                return AnonymousState();

            var claims = ParseClaimsFromJwt(token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }
        catch
        {
            return AnonymousState();
        }
    }

    // Nhận token đầy đủ để parse role claim ngay lập tức (không cần reload trang)
    public void NotifyUserAuthentication(string token)
    {
        try
        {
            var claims = ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
        }
        catch
        {
            NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState()));
        }
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState()));
    }

    private static AuthenticationState AnonymousState()
        => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);

        using var jsonDoc = JsonDocument.Parse(jsonBytes);
        var element = jsonDoc.RootElement;

        foreach (var property in element.EnumerateObject())
        {
            var key = property.Name;
            var value = property.Value;

            // Xử lý Role claim - cần map đúng tên
            if (key == "role" || key == ClaimTypes.Role ||
                key == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            {
                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in value.EnumerateArray())
                        claims.Add(new Claim(ClaimTypes.Role, role.GetString() ?? ""));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, value.GetString() ?? ""));
                }
            }
            // Map sub → NameIdentifier
            else if (key == "sub" || key == JwtRegisteredClaimNames.Sub)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, value.GetString() ?? ""));
            }
            // Map email claim
            else if (key == "email" || key == JwtRegisteredClaimNames.Email)
            {
                claims.Add(new Claim(ClaimTypes.Email, value.GetString() ?? ""));
                claims.Add(new Claim(ClaimTypes.Name, value.GetString() ?? "")); // Name = email
            }
            else
            {
                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                        claims.Add(new Claim(key, item.ToString()));
                }
                else
                {
                    claims.Add(new Claim(key, value.ToString()));
                }
            }
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        int mod = base64.Length % 4;
        if (mod == 2) base64 += "==";
        else if (mod == 3) base64 += "=";
        return Convert.FromBase64String(base64);
    }
}

// Alias để dùng trong namespace
file static class JwtRegisteredClaimNames
{
    public const string Sub = "sub";
    public const string Email = "email";
}
