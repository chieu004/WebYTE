using System;

namespace WebYTE.Application.DTOs.Staff;

public class StaffProfileDto
{
    public Guid StaffId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Thuộc tính Nhân viên
    public string? Position { get; set; }
    public string? Department { get; set; }
}

public class UpdateStaffProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
}
