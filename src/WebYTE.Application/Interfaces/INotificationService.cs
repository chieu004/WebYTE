using WebYTE.Application.DTOs.Notification;

namespace WebYTE.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<NotificationDto?> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedId = null);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task SendAppointmentReminderAsync(Guid appointmentId);
    Task DeleteOldNotificationsAsync(int daysOld = 7);
}
