using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Auth;

namespace WebYTE.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return new AuthResponse { IsSuccess = false, Message = $"API Server Error ({response.StatusCode}): {errorBody}" };
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (result != null && result.IsSuccess)
            {
                // Lưu token vào localStorage
                await _localStorage.SetItemAsync("authToken", result.Token);
                
                // Set token vào HttpClient
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                
                // Notify authentication state changed
                ((JwtAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthentication(result.Token);
            }

            return result ?? new AuthResponse { IsSuccess = false, Message = "Lỗi đọc dữ liệu từ API" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { IsSuccess = false, Message = $"Lỗi kết nối Client (Network/CORS): {ex.Message}" };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return new AuthResponse { IsSuccess = false, Message = $"API Server Error ({response.StatusCode}): {errorBody}" };
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return result ?? new AuthResponse { IsSuccess = false, Message = "Lỗi parse JSON" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { IsSuccess = false, Message = $"Lỗi kết nối Client: {ex.Message}" };
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        ((JwtAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogout();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }
}
