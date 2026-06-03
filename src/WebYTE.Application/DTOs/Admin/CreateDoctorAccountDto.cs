using System;
using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Admin;

public class CreateDoctorAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Gender Gender { get; set; }
    public Guid? SpecialtyId { get; set; }
    public string? Qualifications { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? Bio { get; set; }
}
