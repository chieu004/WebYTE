using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Admin;

public class CreateStaffAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Gender Gender { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
}
