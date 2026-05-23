using System;
using System.Collections.Generic;

namespace WebYTE.Core.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? BloodType { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
