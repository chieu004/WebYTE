using System.Threading.Tasks;
using WebYTE.Application.DTOs.Auth;

namespace WebYTE.Client.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
}
