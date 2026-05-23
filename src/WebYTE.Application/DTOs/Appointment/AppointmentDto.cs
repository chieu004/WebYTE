using System;
using WebYTE.Core.Enums;

namespace WebYTE.Application.DTOs.Appointment;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; } // Thêm DoctorId
    public string DoctorName { get; set; } = string.Empty;
    
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public string? Symptoms { get; set; }
    
    public AppointmentStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}
