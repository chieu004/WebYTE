using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Patient;
using WebYTE.Application.DTOs.Notification;
using WebYTE.Application.Interfaces;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public PatientService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<PatientProfileDto?> GetPatientProfileAsync(Guid userId)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null) return null;

        return new PatientProfileDto
        {
            PatientId = patient.Id,
            UserId = patient.UserId,
            FullName = patient.User.FullName,
            Email = patient.User.Email ?? string.Empty,
            Phone = patient.User.PhoneNumber,
            DateOfBirth = patient.User.DateOfBirth,
            Gender = patient.User.Gender,
            Address = patient.User.Address,
            BloodType = patient.BloodType,
            MedicalHistory = patient.MedicalHistory,
            Allergies = patient.Allergies
        };
    }

    public async Task<bool> UpdatePatientProfileAsync(Guid userId, UpdatePatientProfileDto request)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null) return false;

        // Cập nhật User Info
        patient.User.FullName = request.FullName;
        patient.User.PhoneNumber = request.Phone;
        patient.User.DateOfBirth = request.DateOfBirth;
        patient.User.Gender = request.Gender;
        patient.User.Address = request.Address;

        // Cập nhật Y Tế
        patient.BloodType = request.BloodType;
        patient.MedicalHistory = request.MedicalHistory;
        patient.Allergies = request.Allergies;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DoctorSummaryDto>> GetDoctorsAsync()
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .Select(d => new DoctorSummaryDto
            {
                DoctorId = d.Id,
                FullName = d.User.FullName,
                SpecialtyName = d.Specialty != null ? d.Specialty.Name : "Khác",
                Qualifications = d.Qualifications,
                Experience = d.Experience,
                ConsultationFee = d.ConsultationFee,
                Bio = d.Bio
            }).ToListAsync();
    }

    public async Task<bool> BookAppointmentAsync(Guid userId, BookAppointmentDto request)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return false;

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId);
        if (doctor == null) return false;

        var appointment = new WebYTE.Core.Entities.Appointment
        {
            PatientId = patient.Id,
            DoctorId = request.DoctorId,
            AppointmentDate = request.AppointmentDate,
            Reason = request.Reason,
            Symptoms = request.Symptoms,
            Status = WebYTE.Core.Enums.AppointmentStatus.Pending,
            PaymentStatus = WebYTE.Core.Enums.PaymentStatus.Unpaid
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Gửi thông báo cho Bác sĩ
        await _notificationService.CreateNotificationAsync(
            doctor.UserId,
            "Lịch hẹn mới",
            $"Bệnh nhân {patient.User.FullName} đã đặt lịch khám vào {request.AppointmentDate:dd/MM/yyyy HH:mm}",
            NotificationType.NewAppointment,
            appointment.Id
        );

        // Gửi thông báo cho tất cả Staff (lấy từ role)
        var staffUsers = await _context.Users
            .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { User = u, RoleId = ur.RoleId })
            .Join(_context.Roles, x => x.RoleId, r => r.Id, (x, r) => new { x.User, Role = r })
            .Where(x => x.Role.Name == "Staff")
            .Select(x => x.User)
            .ToListAsync();

        foreach (var staff in staffUsers)
        {
            await _notificationService.CreateNotificationAsync(
                staff.Id,
                "Lịch hẹn mới cần xác nhận",
                $"Bệnh nhân {patient.User.FullName} đã đặt lịch với BS. {doctor.User.FullName} vào {request.AppointmentDate:dd/MM/yyyy HH:mm}",
                NotificationType.NewAppointment,
                appointment.Id
            );
        }

        return true;
    }

    public async Task<List<WebYTE.Application.DTOs.Appointment.AppointmentDto>> GetMyAppointmentsAsync(Guid userId)
    {
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null) return new List<WebYTE.Application.DTOs.Appointment.AppointmentDto>();

        return await _context.Appointments
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.PatientId == patient.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new WebYTE.Application.DTOs.Appointment.AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = "Tôi",
                DoctorName = a.Doctor.User.FullName,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Symptoms = a.Symptoms,
                Status = a.Status,
                PaymentStatus = a.PaymentStatus
            }).ToListAsync();
    }
}
