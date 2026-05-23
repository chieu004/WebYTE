using System;
using System.Collections.Generic;

namespace WebYTE.Core.Entities;

public class MedicalRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    
    // EMR Data - should be encrypted
    public string Diagnosis { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    
    public string? DigitalSignature { get; set; } // Chữ ký số EMR
    
    public Prescription? Prescription { get; set; }
}
