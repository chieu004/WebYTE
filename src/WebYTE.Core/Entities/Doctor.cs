using System;
using System.Collections.Generic;

namespace WebYTE.Core.Entities;

public class Doctor : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public Guid? SpecialtyId { get; set; }  // nullable: Bác sĩ có thể điền sau
    public Specialty? Specialty { get; set; }
    
    public string? Qualifications { get; set; }
    public string? Experience { get; set; } // e.g., "10 years"
    public decimal ConsultationFee { get; set; }
    public string? Bio { get; set; }
    
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
