using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Appointment;
using WebYTE.Application.DTOs.Doctor;
using WebYTE.Application.DTOs.MedicalRecord;
using WebYTE.Application.DTOs.Medication;
using WebYTE.Application.DTOs.Prescription;

namespace WebYTE.Application.Interfaces;

public interface IDoctorService
{
    // Profile
    Task<DoctorProfileDto?> GetDoctorProfileAsync(Guid userId);
    Task<bool> UpdateDoctorProfileAsync(Guid userId, UpdateDoctorProfileDto request);
    
    // Appointments
    Task<List<AppointmentDto>> GetAppointmentsTodayAsync(Guid userId);
    Task<List<AppointmentDto>> GetAllAppointmentsAsync(Guid userId);
    Task<List<AppointmentDto>> GetAppointmentsByDateAsync(Guid userId, DateTime date);
    Task<AppointmentDto?> GetAppointmentDetailAsync(Guid userId, Guid appointmentId);
    Task<bool> UpdateAppointmentStatusAsync(Guid userId, Guid appointmentId, int status);
    
    // Medical Records
    Task<List<MedicalRecordDto>> GetMedicalRecordsByDoctorAsync(Guid userId);
    Task<MedicalRecordDto?> GetMedicalRecordDetailAsync(Guid userId, Guid recordId);
    Task<MedicalRecordDto?> CreateMedicalRecordAsync(Guid userId, CreateMedicalRecordDto request);
    Task<bool> UpdateMedicalRecordAsync(Guid userId, Guid recordId, UpdateMedicalRecordDto request);
    
    // Prescriptions
    Task<PrescriptionDto?> GetPrescriptionByMedicalRecordAsync(Guid userId, Guid medicalRecordId);
    Task<PrescriptionDto?> CreatePrescriptionAsync(Guid userId, CreatePrescriptionDto request);
    
    // Medications
    Task<List<MedicationDto>> GetAllMedicationsAsync();
}
