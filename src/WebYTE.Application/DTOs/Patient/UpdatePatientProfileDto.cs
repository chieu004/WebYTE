using System;
using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Patient;

public class UpdatePatientProfileDto
{
    // Cập nhật thông tin cơ bản
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    
    // Cập nhật thông tin y tế cơ bản
    public string? BloodType { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
}
