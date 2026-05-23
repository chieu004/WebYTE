using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebYTE.Application.DTOs.Notification;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Entities;
using WebYTE.Infrastructure.Data;
using WebYTE.Infrastructure.Hubs;

namespace WebYTE.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = Enum.Parse<NotificationType>(n.Type),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                RelatedId = n.RelatedId
            })
            .ToListAsync();
    }

    public async Task<NotificationDto?> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type.ToString(),
            IsRead = false,
            RelatedId = relatedId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var dto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            RelatedId = notification.RelatedId
        };

        // Send real-time notification via SignalR
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", dto);

        return dto;
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task SendAppointmentReminderAsync(Guid appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null) return;

        var timeUntil = appointment.AppointmentDate - DateTime.Now;
        var timeText = timeUntil.TotalHours < 24 
            ? $"{(int)timeUntil.TotalHours} giờ" 
            : $"{(int)timeUntil.TotalDays} ngày";

        // Notify Patient
        await CreateNotificationAsync(
            appointment.Patient.UserId,
            "Nhắc nhở lịch hẹn",
            $"Bạn có lịch hẹn với BS. {appointment.Doctor.User.FullName} vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} (còn {timeText})",
            NotificationType.AppointmentReminder,
            appointmentId
        );

        // Notify Doctor
        await CreateNotificationAsync(
            appointment.Doctor.UserId,
            "Nhắc nhở lịch hẹn",
            $"Bạn có lịch hẹn với bệnh nhân {appointment.Patient.User.FullName} vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} (còn {timeText})",
            NotificationType.AppointmentReminder,
            appointmentId
        );
    }
}
