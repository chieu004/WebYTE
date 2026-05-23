using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Appointment;
using WebYTE.Application.DTOs.Doctor;
using WebYTE.Application.DTOs.MedicalRecord;
using WebYTE.Application.DTOs.Medication;
using WebYTE.Application.DTOs.Prescription;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Entities;
using WebYTE.Core.Enums;
using WebYTE.Infrastructure.Data;

namespace WebYTE.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly ApplicationDbContext _context;

    public DoctorService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==================== PROFILE ====================
    
    public async Task<DoctorProfileDto?> GetDoctorProfileAsync(Guid userId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.UserId == userId);

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

    public async Task<bool> UpdateDoctorProfileAsync(Guid userId, UpdateDoctorProfileDto request)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);

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

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== APPOINTMENTS ====================
    
    public async Task<List<AppointmentDto>> GetAppointmentsTodayAsync(Guid userId)
    {
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null)
        {
            Console.WriteLine($"[DEBUG] Doctor not found for UserId: {userId}");
            return new List<AppointmentDto>();
        }

        Console.WriteLine($"[DEBUG] Doctor found: {doctor.Id}, UserId: {userId}");
        Console.WriteLine($"[DEBUG] Querying appointments for today: {today} to {tomorrow}");

        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Where(a => a.DoctorId == doctor.Id 
                     && a.AppointmentDate >= today 
                     && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Symptoms = a.Symptoms,
                Status = a.Status,
                PaymentStatus = a.PaymentStatus
            }).ToListAsync();

        Console.WriteLine($"[DEBUG] Found {appointments.Count} appointments for today");
        return appointments;
    }

    public async Task<List<AppointmentDto>> GetAllAppointmentsAsync(Guid userId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return new List<AppointmentDto>();

        return await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Where(a => a.DoctorId == doctor.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Symptoms = a.Symptoms,
                Status = a.Status,
                PaymentStatus = a.PaymentStatus
            }).ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetAppointmentsByDateAsync(Guid userId, DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return new List<AppointmentDto>();

        return await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Where(a => a.DoctorId == doctor.Id 
                     && a.AppointmentDate >= startDate 
                     && a.AppointmentDate < endDate)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Symptoms = a.Symptoms,
                Status = a.Status,
                PaymentStatus = a.PaymentStatus
            }).ToListAsync();
    }

    public async Task<AppointmentDto?> GetAppointmentDetailAsync(Guid userId, Guid appointmentId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return null;

        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

        if (appointment == null) return null;

        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient.User.FullName,
            AppointmentDate = appointment.AppointmentDate,
            Reason = appointment.Reason,
            Symptoms = appointment.Symptoms,
            Status = appointment.Status,
            PaymentStatus = appointment.PaymentStatus
        };
    }

    public async Task<bool> UpdateAppointmentStatusAsync(Guid userId, Guid appointmentId, int status)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return false;

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

        if (appointment == null) return false;

        appointment.Status = (AppointmentStatus)status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== MEDICAL RECORDS ====================
    
    public async Task<List<MedicalRecordDto>> GetMedicalRecordsByDoctorAsync(Guid userId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return new List<MedicalRecordDto>();

        return await _context.MedicalRecords
            .Include(m => m.Patient)
            .ThenInclude(p => p.User)
            .Include(m => m.Doctor)
            .ThenInclude(d => d.User)
            .Include(m => m.Appointment)
            .Include(m => m.Prescription)
            .Where(m => m.DoctorId == doctor.Id)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                PatientName = m.Patient.User.FullName,
                AppointmentId = m.AppointmentId,
                AppointmentDate = m.Appointment.AppointmentDate,
                DoctorId = m.DoctorId,
                DoctorName = m.Doctor.User.FullName,
                Diagnosis = m.Diagnosis,
                TreatmentPlan = m.TreatmentPlan,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt,
                HasPrescription = m.Prescription != null
            }).ToListAsync();
    }

    public async Task<MedicalRecordDto?> GetMedicalRecordDetailAsync(Guid userId, Guid recordId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return null;

        var record = await _context.MedicalRecords
            .Include(m => m.Patient)
            .ThenInclude(p => p.User)
            .Include(m => m.Doctor)
            .ThenInclude(d => d.User)
            .Include(m => m.Appointment)
            .Include(m => m.Prescription)
            .FirstOrDefaultAsync(m => m.Id == recordId && m.DoctorId == doctor.Id);

        if (record == null) return null;

        return new MedicalRecordDto
        {
            Id = record.Id,
            PatientId = record.PatientId,
            PatientName = record.Patient.User.FullName,
            AppointmentId = record.AppointmentId,
            AppointmentDate = record.Appointment.AppointmentDate,
            DoctorId = record.DoctorId,
            DoctorName = record.Doctor.User.FullName,
            Diagnosis = record.Diagnosis,
            TreatmentPlan = record.TreatmentPlan,
            Notes = record.Notes,
            CreatedAt = record.CreatedAt,
            HasPrescription = record.Prescription != null
        };
    }

    public async Task<MedicalRecordDto?> CreateMedicalRecordAsync(Guid userId, CreateMedicalRecordDto request)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return null;

        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.DoctorId == doctor.Id);

        if (appointment == null) return null;

        var record = new MedicalRecord
        {
            PatientId = appointment.PatientId,
            AppointmentId = appointment.Id,
            DoctorId = doctor.Id,
            Diagnosis = request.Diagnosis,
            TreatmentPlan = request.TreatmentPlan,
            Notes = request.Notes
        };

        _context.MedicalRecords.Add(record);
        
        // Update appointment status to Completed
        appointment.Status = AppointmentStatus.Completed;
        
        await _context.SaveChangesAsync();

        return new MedicalRecordDto
        {
            Id = record.Id,
            PatientId = record.PatientId,
            PatientName = appointment.Patient.User.FullName,
            AppointmentId = record.AppointmentId,
            AppointmentDate = appointment.AppointmentDate,
            DoctorId = record.DoctorId,
            DoctorName = doctor.User.FullName,
            Diagnosis = record.Diagnosis,
            TreatmentPlan = record.TreatmentPlan,
            Notes = record.Notes,
            CreatedAt = record.CreatedAt,
            HasPrescription = false
        };
    }

    public async Task<bool> UpdateMedicalRecordAsync(Guid userId, Guid recordId, UpdateMedicalRecordDto request)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return false;

        var record = await _context.MedicalRecords
            .FirstOrDefaultAsync(m => m.Id == recordId && m.DoctorId == doctor.Id);

        if (record == null) return false;

        record.Diagnosis = request.Diagnosis;
        record.TreatmentPlan = request.TreatmentPlan;
        record.Notes = request.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== PRESCRIPTIONS ====================
    
    public async Task<PrescriptionDto?> GetPrescriptionByMedicalRecordAsync(Guid userId, Guid medicalRecordId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return null;

        var prescription = await _context.Prescriptions
            .Include(p => p.Details)
            .ThenInclude(d => d.Medication)
            .FirstOrDefaultAsync(p => p.MedicalRecordId == medicalRecordId);

        if (prescription == null) return null;

        // Verify doctor owns this medical record
        var record = await _context.MedicalRecords
            .FirstOrDefaultAsync(m => m.Id == medicalRecordId && m.DoctorId == doctor.Id);
        
        if (record == null) return null;

        return new PrescriptionDto
        {
            Id = prescription.Id,
            MedicalRecordId = prescription.MedicalRecordId,
            Notes = prescription.Notes,
            Details = prescription.Details.Select(d => new PrescriptionDetailDto
            {
                Id = d.Id,
                MedicationId = d.MedicationId,
                MedicationName = d.Medication.Name,
                Quantity = d.Quantity,
                Dosage = d.Dosage,
                Instructions = d.Instructions
            }).ToList()
        };
    }

    public async Task<PrescriptionDto?> CreatePrescriptionAsync(Guid userId, CreatePrescriptionDto request)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return null;

        // Verify doctor owns this medical record
        var record = await _context.MedicalRecords
            .FirstOrDefaultAsync(m => m.Id == request.MedicalRecordId && m.DoctorId == doctor.Id);
        
        if (record == null) return null;

        var prescription = new Core.Entities.Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            Notes = request.Notes
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        // Add details
        foreach (var detail in request.Details)
        {
            var prescriptionDetail = new Core.Entities.PrescriptionDetail
            {
                PrescriptionId = prescription.Id,
                MedicationId = detail.MedicationId,
                Quantity = detail.Quantity,
                Dosage = detail.Dosage,
                Instructions = detail.Instructions
            };
            _context.PrescriptionDetails.Add(prescriptionDetail);
        }

        await _context.SaveChangesAsync();

        // Load medications for response
        var createdPrescription = await _context.Prescriptions
            .Include(p => p.Details)
            .ThenInclude(d => d.Medication)
            .FirstOrDefaultAsync(p => p.Id == prescription.Id);

        return new PrescriptionDto
        {
            Id = createdPrescription!.Id,
            MedicalRecordId = createdPrescription.MedicalRecordId,
            Notes = createdPrescription.Notes,
            Details = createdPrescription.Details.Select(d => new PrescriptionDetailDto
            {
                Id = d.Id,
                MedicationId = d.MedicationId,
                MedicationName = d.Medication.Name,
                Quantity = d.Quantity,
                Dosage = d.Dosage,
                Instructions = d.Instructions
            }).ToList()
        };
    }

    // ==================== MEDICATIONS ====================
    
    public async Task<List<MedicationDto>> GetAllMedicationsAsync()
    {
        return await _context.Medications
            .OrderBy(m => m.Name)
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
}
