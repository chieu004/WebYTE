using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebYTE.Core.Enums;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentReminderService> _logger;

    public AppointmentReminderService(
        IServiceProvider serviceProvider,
        ILogger<AppointmentReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Reminder Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendReminders();
                
                // Xóa thông báo cũ đã đọc (cũ hơn 7 ngày)
                await DeleteOldNotifications();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Appointment Reminder Service");
            }

            // Check every 30 minutes
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task DeleteOldNotifications()
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.INotificationService>();
        
        await notificationService.DeleteOldNotificationsAsync(7); // Xóa thông báo đã đọc cũ hơn 7 ngày
    }

    private async Task CheckAndSendReminders()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.INotificationService>();

        var now = DateTime.Now;
        var tomorrow = now.AddDays(1);
        var in2Hours = now.AddHours(2);

        // Find appointments that need reminders
        // 1. Appointments in next 24 hours (send once per day)
        // 2. Appointments in next 2 hours (send once)
        var upcomingAppointments = await context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.Status == AppointmentStatus.Confirmed 
                     && a.AppointmentDate > now 
                     && a.AppointmentDate <= tomorrow)
            .ToListAsync();

        foreach (var appointment in upcomingAppointments)
        {
            // Check if reminder already sent today
            var reminderSentToday = await context.Notifications
                .AnyAsync(n => n.RelatedId == appointment.Id 
                            && n.Type == "AppointmentReminder"
                            && n.CreatedAt.Date == now.Date);

            if (!reminderSentToday)
            {
                await notificationService.SendAppointmentReminderAsync(appointment.Id);
                _logger.LogInformation($"Sent reminder for appointment {appointment.Id}");
            }
        }

        // Send urgent reminder for appointments in next 2 hours
        var urgentAppointments = upcomingAppointments
            .Where(a => a.AppointmentDate <= in2Hours)
            .ToList();

        foreach (var appointment in urgentAppointments)
        {
            // Check if urgent reminder already sent
            var urgentReminderSent = await context.Notifications
                .AnyAsync(n => n.RelatedId == appointment.Id 
                            && n.Type == "AppointmentReminder"
                            && n.Message.Contains("còn") 
                            && n.CreatedAt > now.AddHours(-2));

            if (!urgentReminderSent)
            {
                var timeUntil = appointment.AppointmentDate - now;
                var minutes = (int)timeUntil.TotalMinutes;

                // Send urgent notification
                await notificationService.CreateNotificationAsync(
                    appointment.Patient.UserId,
                    "⚠️ Lịch hẹn sắp tới!",
                    $"Lịch hẹn của bạn với BS. {appointment.Doctor.User.FullName} sẽ bắt đầu trong {minutes} phút!",
                    Application.DTOs.Notification.NotificationType.AppointmentReminder,
                    appointment.Id
                );

                await notificationService.CreateNotificationAsync(
                    appointment.Doctor.UserId,
                    "⚠️ Lịch hẹn sắp tới!",
                    $"Lịch hẹn với bệnh nhân {appointment.Patient.User.FullName} sẽ bắt đầu trong {minutes} phút!",
                    Application.DTOs.Notification.NotificationType.AppointmentReminder,
                    appointment.Id
                );

                _logger.LogInformation($"Sent urgent reminder for appointment {appointment.Id}");
            }
        }
    }
}
