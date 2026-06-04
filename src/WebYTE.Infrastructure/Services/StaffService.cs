using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Appointment;
using WebYTE.Application.DTOs.Notification;
using WebYTE.Application.DTOs.Staff;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Enums;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class StaffService : IStaffService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public StaffService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<StaffProfileDto?> GetProfileAsync(Guid userId)
    {
        var staff = await _context.Staffs
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (staff == null) return null;

        return new StaffProfileDto
        {
            StaffId = staff.Id,
            UserId = staff.UserId,
            FullName = staff.User.FullName,
            Email = staff.User.Email ?? string.Empty,
            Phone = staff.User.PhoneNumber,
            Position = staff.Position,
            Department = staff.Department
        };
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateStaffProfileDto request)
    {
        var staff = await _context.Staffs
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (staff == null) return false;

        staff.User.FullName = request.FullName;
        staff.User.PhoneNumber = request.Phone;
        staff.Position = request.Position;
        staff.Department = request.Department;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AppointmentDto>> GetAllPendingAppointmentsAsync()
    {
        // Lấy danh sách lịch hẹn cần duyệt
        return await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor) // Thêm Include Doctor
            .ThenInclude(d => d.User) // Thêm Include Doctor.User
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName, // Thêm DoctorName
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Symptoms = a.Symptoms,
                Status = a.Status,
                PaymentStatus = a.PaymentStatus
            }).ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetAllAppointmentsAsync()
    {
        // Lấy tất cả lịch hẹn
        return await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Symptoms = a.Symptoms,
                Status = a.Status,
                PaymentStatus = a.PaymentStatus
            }).ToListAsync();
    }

    public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus status)
    {
        Console.WriteLine($"[DEBUG] Service - Finding appointment: {appointmentId}");
        
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
        
        if (appointment == null)
        {
            Console.WriteLine($"[DEBUG] Service - Appointment NOT FOUND: {appointmentId}");
            return false;
        }

        Console.WriteLine($"[DEBUG] Service - Found appointment. Current status: {appointment.Status}, New status: {status}");
        
        appointment.Status = status;
        
        var changes = await _context.SaveChangesAsync();
        
        Console.WriteLine($"[DEBUG] Service - SaveChanges result: {changes} rows affected");

        // Gửi thông báo dựa trên trạng thái
        if (status == AppointmentStatus.Confirmed)
        {
            // Thông báo cho bệnh nhân
            await _notificationService.CreateNotificationAsync(
                appointment.Patient.UserId,
                "Lịch hẹn đã được xác nhận",
                $"Lịch hẹn với BS. {appointment.Doctor.User.FullName} vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} đã được xác nhận",
                NotificationType.AppointmentConfirmed,
                appointmentId
            );

            // Thông báo cho bác sĩ
            await _notificationService.CreateNotificationAsync(
                appointment.Doctor.UserId,
                "Lịch hẹn đã được xác nhận",
                $"Lịch hẹn với bệnh nhân {appointment.Patient.User.FullName} vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} đã được xác nhận",
                NotificationType.AppointmentConfirmed,
                appointmentId
            );
        }
        else if (status == AppointmentStatus.Cancelled)
        {
            // Thông báo hủy lịch
            await _notificationService.CreateNotificationAsync(
                appointment.Patient.UserId,
                "Lịch hẹn đã bị hủy",
                $"Lịch hẹn với BS. {appointment.Doctor.User.FullName} vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} đã bị hủy",
                NotificationType.AppointmentCancelled,
                appointmentId
            );

            await _notificationService.CreateNotificationAsync(
                appointment.Doctor.UserId,
                "Lịch hẹn đã bị hủy",
                $"Lịch hẹn với bệnh nhân {appointment.Patient.User.FullName} vào {appointment.AppointmentDate:dd/MM/yyyy HH:mm} đã bị hủy",
                NotificationType.AppointmentCancelled,
                appointmentId
            );
        }
        
        return true;
    }

    public async Task<bool> ProcessPaymentAsync(Guid appointmentId, PaymentStatus paymentStatus)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null) return false;

        appointment.PaymentStatus = paymentStatus;
        await _context.SaveChangesAsync();
        return true;
    }
}
