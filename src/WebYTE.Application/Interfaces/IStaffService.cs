using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Appointment;
using WebYTE.Application.DTOs.Staff;
using WebYTE.Core.Enums;

namespace WebYTE.Application.Interfaces;

public interface IStaffService
{
    Task<StaffProfileDto?> GetProfileAsync(Guid userId);
    Task<bool> UpdateProfileAsync(Guid userId, UpdateStaffProfileDto request);
    
    Task<List<AppointmentDto>> GetAllPendingAppointmentsAsync();
    Task<List<AppointmentDto>> GetAllAppointmentsAsync(); // Thêm method mới
    Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus status);
    Task<bool> ProcessPaymentAsync(Guid appointmentId, PaymentStatus paymentStatus);
}
