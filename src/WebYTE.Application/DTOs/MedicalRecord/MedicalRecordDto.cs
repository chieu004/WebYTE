using System;

namespace WebYTE.Application.DTOs.MedicalRecord;

public class MedicalRecordDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasPrescription { get; set; }
}

public class CreateMedicalRecordDto
{
    public Guid AppointmentId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
}

public class UpdateMedicalRecordDto
{
    public string Diagnosis { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
}
