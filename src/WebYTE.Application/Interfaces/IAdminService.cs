using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Admin;
using WebYTE.Application.DTOs.Doctor;
using WebYTE.Application.DTOs.Medication;
using WebYTE.Application.DTOs.Patient;
using WebYTE.Application.DTOs.Staff;

namespace WebYTE.Application.Interfaces;

public interface IAdminService
{
    Task<List<ManageUserDto>> GetAllUsersAsync();
    Task<bool> ToggleUserStatusAsync(Guid userId);
    
    Task<List<SpecialtyDto>> GetAllSpecialtiesAsync();
    Task<SpecialtyDto?> CreateSpecialtyAsync(SpecialtyDto request);

    // Tạo tài khoản bác sĩ / nhân viên
    Task<(bool Success, string Message)> CreateDoctorAccountAsync(CreateDoctorAccountDto request);
    Task<(bool Success, string Message)> CreateStaffAccountAsync(CreateStaffAccountDto request);

    // Quản lý thuốc
    Task<List<MedicationDto>> GetAllMedicationsAsync();
    Task<MedicationDto?> CreateMedicationAsync(MedicationDto request);
    Task<bool> UpdateMedicationAsync(Guid id, MedicationDto request);
    Task<bool> DeleteMedicationAsync(Guid id);
    
    // Quản lý Doctor
    Task<List<DoctorProfileDto>> GetAllDoctorsAsync();
    Task<DoctorProfileDto?> GetDoctorByIdAsync(Guid doctorId);
    Task<bool> UpdateDoctorAsync(Guid doctorId, UpdateDoctorProfileDto request);
    Task<bool> DeleteDoctorAsync(Guid doctorId);
    
    // Quản lý Patient
    Task<List<PatientProfileDto>> GetAllPatientsAsync();
    Task<PatientProfileDto?> GetPatientByIdAsync(Guid patientId);
    Task<bool> UpdatePatientAsync(Guid patientId, UpdatePatientProfileDto request);
    Task<bool> DeletePatientAsync(Guid patientId);
    
    // Quản lý Staff
    Task<List<StaffProfileDto>> GetAllStaffsAsync();
    Task<StaffProfileDto?> GetStaffByIdAsync(Guid staffId);
    Task<bool> UpdateStaffAsync(Guid staffId, UpdateStaffProfileDto request);
    Task<bool> DeleteStaffAsync(Guid staffId);
    
    // Thống kê
    Task<UserStatisticsDto> GetUserStatisticsAsync();
}
