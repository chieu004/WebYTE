using System;

namespace WebYTE.Application.DTOs.Patient;

public class BookAppointmentDto
{
    public Guid DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public string? Symptoms { get; set; }
}
