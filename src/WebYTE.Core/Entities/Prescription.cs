using System;
using System.Collections.Generic;

namespace WebYTE.Core.Entities;

public class Prescription : BaseEntity
{
    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
    
    public string? Notes { get; set; }
    
    public ICollection<PrescriptionDetail> Details { get; set; } = new List<PrescriptionDetail>();
}

public class PrescriptionDetail : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    
    public Guid MedicationId { get; set; }
    public Medication Medication { get; set; } = null!;
    
    public int Quantity { get; set; }
    public string Dosage { get; set; } = string.Empty; // e.g., "1 viên/lần, 2 lần/ngày"
    public string? Instructions { get; set; }
}
