using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using WebYTE.Client;
using WebYTE.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Đăng ký LocalStorageService trước
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Kết nối đến WebYTE.API Backend
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5109/") });

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NotificationService>();

var host = builder.Build();

// Tự động load token từ localStorage khi app khởi động
var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
var httpClient = host.Services.GetRequiredService<HttpClient>();

try
{
    var token = await localStorage.GetItemAsync<string>("authToken");
    if (!string.IsNullOrWhiteSpace(token))
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
catch { /* Bỏ qua nếu localStorage chưa khả dụng */ }

await host.RunAsync();
