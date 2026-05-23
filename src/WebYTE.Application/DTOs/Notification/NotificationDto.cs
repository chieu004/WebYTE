namespace WebYTE.Application.DTOs.Notification;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? RelatedId { get; set; } // AppointmentId, MedicalRecordId, etc.
}

public enum NotificationType
{
    AppointmentReminder,
    AppointmentConfirmed,
    AppointmentCancelled,
    MedicalRecordCreated,
    PrescriptionCreated,
    General
}
