using System;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Patient;

namespace WebYTE.Application.Interfaces;

public interface IPatientService
{
    Task<PatientProfileDto?> GetPatientProfileAsync(Guid userId);
    Task<bool> UpdatePatientProfileAsync(Guid userId, UpdatePatientProfileDto request);
    
    // Thêm chức năng đặt lịch
    Task<System.Collections.Generic.List<DoctorSummaryDto>> GetDoctorsAsync();
    Task<bool> BookAppointmentAsync(Guid userId, BookAppointmentDto request);
    Task<System.Collections.Generic.List<WebYTE.Application.DTOs.Appointment.AppointmentDto>> GetMyAppointmentsAsync(Guid userId);
}
