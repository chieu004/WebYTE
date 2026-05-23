using System;
using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Patient;

public class PatientProfileDto
{
    public Guid PatientId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    
    // Y Tế
    public string? BloodType { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
}
