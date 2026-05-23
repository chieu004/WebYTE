using System;
using System.Collections.Generic;

namespace WebYTE.Application.DTOs.Prescription;

public class PrescriptionDto
{
    public Guid Id { get; set; }
    public Guid MedicalRecordId { get; set; }
    public string? Notes { get; set; }
    public List<PrescriptionDetailDto> Details { get; set; } = new();
}

public class PrescriptionDetailDto
{
    public Guid Id { get; set; }
    public Guid MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class CreatePrescriptionDto
{
    public Guid MedicalRecordId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePrescriptionDetailDto> Details { get; set; } = new();
}

public class CreatePrescriptionDetailDto
{
    public Guid MedicationId { get; set; }
    public int Quantity { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}
