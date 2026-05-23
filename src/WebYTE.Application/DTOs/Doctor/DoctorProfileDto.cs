using System;
using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Doctor;

public class DoctorProfileDto
{
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Gender Gender { get; set; }
    
    // Y Tế
    public Guid SpecialtyId { get; set; }
    public string? SpecialtyName { get; set; }
    public string? Qualifications { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? Bio { get; set; }
}
