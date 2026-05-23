using System;

namespace WebYTE.Application.DTOs.Doctor;

public class UpdateDoctorProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Y Tế
    public Guid SpecialtyId { get; set; }
    public string? Qualifications { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? Bio { get; set; }
}
