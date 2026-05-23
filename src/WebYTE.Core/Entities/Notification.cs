namespace WebYTE.Core.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // AppointmentReminder, AppointmentConfirmed, etc.
    public bool IsRead { get; set; }
    public Guid? RelatedId { get; set; } // AppointmentId, MedicalRecordId, etc.
    
    // Navigation
    public ApplicationUser User { get; set; } = null!;
}
