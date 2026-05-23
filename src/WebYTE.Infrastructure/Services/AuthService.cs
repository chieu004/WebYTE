using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Auth;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Entities;
using WebYTE.Core.Enums;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse { IsSuccess = false, Message = "Tài khoản không tồn tại." };
        }

        // Kiểm tra tài khoản bị khóa
        if (await _userManager.IsLockedOutAsync(user))
        {
            return new AuthResponse 
            { 
                IsSuccess = false, 
                Message = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên." 
            };
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            return new AuthResponse { IsSuccess = false, Message = "Sai mật khẩu." };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return new AuthResponse
        {
            IsSuccess = true,
            Message = "Đăng nhập thành công",
            Token = token,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = roles.Count > 0 ? roles[0] : string.Empty
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
        {
            return new AuthResponse { IsSuccess = false, Message = "Email đã được sử dụng." };
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            PhoneNumber = request.Phone,
            UserRole = request.Role
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponse { IsSuccess = false, Message = "Tạo tài khoản thất bại." };
        }

        // Add Role
        string roleName = request.Role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
        }
        await _userManager.AddToRoleAsync(user, roleName);

        // Tạo Profile tương ứng dựa vào Role
        // (Đây là Transaction nhỏ)
        if (request.Role == RoleType.Patient)
        {
            _context.Patients.Add(new Patient { UserId = user.Id });
        }
        else if (request.Role == RoleType.Doctor)
        {
            // SpecialtyId sẽ được cập nhật sau khi Bác sĩ điền hồ sơ
            _context.Doctors.Add(new Doctor { UserId = user.Id });
        }
        else if (request.Role == RoleType.Staff)
        {
             _context.Staffs.Add(new Staff { UserId = user.Id });
        }

        await _context.SaveChangesAsync();

        return new AuthResponse { IsSuccess = true, Message = "Đăng ký thành công. Vui lòng đăng nhập." };
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "super_secret_key_webyte_2026_!@#$%");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("FullName", user.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
