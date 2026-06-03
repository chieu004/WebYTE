using System;
using System.Collections.Generic;
using WebYTE.Core.Enums;

namespace WebYTE.Core.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public string? Symptoms { get; set; } // Can be populated by AI Triage
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    
    public MedicalRecord? MedicalRecord { get; set; }
    
    // Invoice relationship
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
