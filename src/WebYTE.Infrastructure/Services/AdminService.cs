using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Admin;
using WebYTE.Application.DTOs.Doctor;
using WebYTE.Application.DTOs.Medication;
using WebYTE.Application.DTOs.Patient;
using WebYTE.Application.DTOs.Staff;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Entities;
using WebYTE.Core.Enums;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<ManageUserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<ManageUserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new ManageUserDto
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Unknown",
                IsActive = user.IsActive
            });
        }
        return result;
    }

    public async Task<bool> ToggleUserStatusAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        // Toggle IsActive
        user.IsActive = !user.IsActive;
        
        // Nếu IsActive = false → Khóa tài khoản (lockout đến năm 9999)
        // Nếu IsActive = true → Mở khóa tài khoản
        if (!user.IsActive)
        {
            // Khóa tài khoản
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            await _userManager.SetLockoutEnabledAsync(user, true);
        }
        else
        {
            // Mở khóa tài khoản
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<List<SpecialtyDto>> GetAllSpecialtiesAsync()
    {
        return await _context.Specialties
            .Select(s => new SpecialtyDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            }).ToListAsync();
    }

    public async Task<SpecialtyDto?> CreateSpecialtyAsync(SpecialtyDto request)
    {
        var spec = new Specialty
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Specialties.Add(spec);
        await _context.SaveChangesAsync();

        request.Id = spec.Id;
        return request;
    }

    // ==================== QUẢN LÝ DOCTOR ====================
    
    public async Task<List<DoctorProfileDto>> GetAllDoctorsAsync()
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .Where(d => !d.IsDeleted)
            .Select(d => new DoctorProfileDto
            {
                DoctorId = d.Id,
                UserId = d.UserId,
                FullName = d.User.FullName,
                Email = d.User.Email ?? string.Empty,
                Phone = d.User.PhoneNumber,
                Gender = d.User.Gender,
                SpecialtyId = d.SpecialtyId ?? Guid.Empty,
                SpecialtyName = d.Specialty != null ? d.Specialty.Name : null,
                Qualifications = d.Qualifications,
                Experience = d.Experience,
                ConsultationFee = d.ConsultationFee,
                Bio = d.Bio
            }).ToListAsync();
    }

    public async Task<DoctorProfileDto?> GetDoctorByIdAsync(Guid doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

        if (doctor == null) return null;

        return new DoctorProfileDto
        {
            DoctorId = doctor.Id,
            UserId = doctor.UserId,
            FullName = doctor.User.FullName,
            Email = doctor.User.Email ?? string.Empty,
            Phone = doctor.User.PhoneNumber,
            Gender = doctor.User.Gender,
            SpecialtyId = doctor.SpecialtyId ?? Guid.Empty,
            SpecialtyName = doctor.Specialty?.Name,
            Qualifications = doctor.Qualifications,
            Experience = doctor.Experience,
            ConsultationFee = doctor.ConsultationFee,
            Bio = doctor.Bio
        };
    }

    public async Task<bool> UpdateDoctorAsync(Guid doctorId, UpdateDoctorProfileDto request)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

        if (doctor == null) return false;

        doctor.User.FullName = request.FullName;
        doctor.User.PhoneNumber = request.Phone;

        if (request.SpecialtyId != Guid.Empty)
        {
            doctor.SpecialtyId = request.SpecialtyId;
        }

        doctor.Qualifications = request.Qualifications;
        doctor.Experience = request.Experience;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.Bio = request.Bio;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDoctorAsync(Guid doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        if (doctor == null) return false;

        doctor.IsDeleted = true;
        doctor.User.IsActive = false;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== QUẢN LÝ PATIENT ====================
    
    public async Task<List<PatientProfileDto>> GetAllPatientsAsync()
    {
        return await _context.Patients
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .Select(p => new PatientProfileDto
            {
                PatientId = p.Id,
                UserId = p.UserId,
                FullName = p.User.FullName,
                Email = p.User.Email ?? string.Empty,
                Phone = p.User.PhoneNumber,
                DateOfBirth = p.User.DateOfBirth,
                Gender = p.User.Gender,
                Address = p.User.Address,
                BloodType = p.BloodType,
                MedicalHistory = p.MedicalHistory,
                Allergies = p.Allergies
            }).ToListAsync();
    }

    public async Task<PatientProfileDto?> GetPatientByIdAsync(Guid patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted);

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

    public async Task<bool> UpdatePatientAsync(Guid patientId, UpdatePatientProfileDto request)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted);

        if (patient == null) return false;

        patient.User.FullName = request.FullName;
        patient.User.PhoneNumber = request.Phone;
        patient.User.DateOfBirth = request.DateOfBirth;
        patient.User.Gender = request.Gender;
        patient.User.Address = request.Address;
        patient.BloodType = request.BloodType;
        patient.MedicalHistory = request.MedicalHistory;
        patient.Allergies = request.Allergies;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePatientAsync(Guid patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null) return false;

        patient.IsDeleted = true;
        patient.User.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== QUẢN LÝ STAFF ====================
    
    public async Task<List<StaffProfileDto>> GetAllStaffsAsync()
    {
        return await _context.Staffs
            .Include(s => s.User)
            .Where(s => !s.IsDeleted)
            .Select(s => new StaffProfileDto
            {
                StaffId = s.Id,
                UserId = s.UserId,
                FullName = s.User.FullName,
                Email = s.User.Email ?? string.Empty,
                Phone = s.User.PhoneNumber,
                Position = s.Position,
                Department = s.Department
            }).ToListAsync();
    }

    public async Task<StaffProfileDto?> GetStaffByIdAsync(Guid staffId)
    {
        var staff = await _context.Staffs
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == staffId && !s.IsDeleted);

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

    public async Task<bool> UpdateStaffAsync(Guid staffId, UpdateStaffProfileDto request)
    {
        var staff = await _context.Staffs
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == staffId && !s.IsDeleted);

        if (staff == null) return false;

        staff.User.FullName = request.FullName;
        staff.User.PhoneNumber = request.Phone;
        staff.Position = request.Position;
        staff.Department = request.Department;
        staff.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteStaffAsync(Guid staffId)
    {
        var staff = await _context.Staffs
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == staffId);

        if (staff == null) return false;

        staff.IsDeleted = true;
        staff.User.IsActive = false;
        staff.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== TẠO TÀI KHOẢN BÁC SĨ / NHÂN VIÊN ====================

    public async Task<(bool Success, string Message)> CreateDoctorAccountAsync(CreateDoctorAccountDto request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return (false, "Email đã được sử dụng.");

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.Phone,
            Gender = request.Gender,
            UserRole = RoleType.Doctor,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync("Doctor"))
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Doctor" });
        await _userManager.AddToRoleAsync(user, "Doctor");

        _context.Doctors.Add(new Doctor
        {
            UserId = user.Id,
            SpecialtyId = request.SpecialtyId,
            Qualifications = request.Qualifications,
            Experience = request.Experience,
            ConsultationFee = request.ConsultationFee,
            Bio = request.Bio
        });

        await _context.SaveChangesAsync();
        return (true, "Tạo tài khoản bác sĩ thành công.");
    }

    public async Task<(bool Success, string Message)> CreateStaffAccountAsync(CreateStaffAccountDto request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return (false, "Email đã được sử dụng.");

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.Phone,
            Gender = request.Gender,
            UserRole = RoleType.Staff,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync("Staff"))
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Staff" });
        await _userManager.AddToRoleAsync(user, "Staff");

        _context.Staffs.Add(new Staff
        {
            UserId = user.Id,
            Position = request.Position,
            Department = request.Department
        });

        await _context.SaveChangesAsync();
        return (true, "Tạo tài khoản nhân viên thành công.");
    }

    // ==================== QUẢN LÝ THUỐC ====================

    public async Task<List<MedicationDto>> GetAllMedicationsAsync()
    {
        return await _context.Medications
            .Where(m => !m.IsDeleted)
            .Select(m => new MedicationDto
            {
                Id = m.Id,
                Name = m.Name,
                GenericName = m.GenericName,
                Usage = m.Usage,
                SideEffects = m.SideEffects,
                Unit = m.Unit,
                Price = m.Price,
                InStock = m.InStock
            }).ToListAsync();
    }

    public async Task<MedicationDto?> CreateMedicationAsync(MedicationDto request)
    {
        var med = new Medication
        {
            Name = request.Name,
            GenericName = request.GenericName,
            Usage = request.Usage,
            SideEffects = request.SideEffects,
            Unit = request.Unit,
            Price = request.Price,
            InStock = request.InStock
        };
        _context.Medications.Add(med);
        await _context.SaveChangesAsync();
        request.Id = med.Id;
        return request;
    }

    public async Task<bool> UpdateMedicationAsync(Guid id, MedicationDto request)
    {
        var med = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        if (med == null) return false;

        med.Name = request.Name;
        med.GenericName = request.GenericName;
        med.Usage = request.Usage;
        med.SideEffects = request.SideEffects;
        med.Unit = request.Unit;
        med.Price = request.Price;
        med.InStock = request.InStock;
        med.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMedicationAsync(Guid id)
    {
        var med = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id);
        if (med == null) return false;
        med.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== THỐNG KÊ ====================
    
    public async Task<UserStatisticsDto> GetUserStatisticsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
        var inactiveUsers = totalUsers - activeUsers;

        var totalDoctors = await _context.Doctors.CountAsync(d => !d.IsDeleted);
        var totalPatients = await _context.Patients.CountAsync(p => !p.IsDeleted);
        var totalStaff = await _context.Staffs.CountAsync(s => !s.IsDeleted);

        return new UserStatisticsDto
        {
            TotalUsers = totalUsers,
            TotalDoctors = totalDoctors,
            TotalPatients = totalPatients,
            TotalStaff = totalStaff,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers
        };
    }
}
