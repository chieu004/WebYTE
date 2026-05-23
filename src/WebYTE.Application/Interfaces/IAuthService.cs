using System.Threading.Tasks;
using WebYTE.Application.DTOs.Auth;

namespace WebYTE.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}
