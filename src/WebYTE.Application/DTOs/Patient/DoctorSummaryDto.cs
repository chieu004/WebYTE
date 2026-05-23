using System;

namespace WebYTE.Application.DTOs.Patient;

public class DoctorSummaryDto
{
    public Guid DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? SpecialtyName { get; set; }
    public string? Qualifications { get; set; }
    public string? Experience { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? Bio { get; set; }
}
