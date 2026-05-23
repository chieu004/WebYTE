using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Doctor;
using WebYTE.Application.DTOs.MedicalRecord;
using WebYTE.Application.DTOs.Prescription;
using WebYTE.Application.Interfaces;

namespace WebYTE.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Doctor")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    // ==================== PROFILE ====================
    
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var profile = await _doctorService.GetDoctorProfileAsync(userId);
        if (profile == null)
            return NotFound("Không tìm thấy thông tin Bác sĩ.");

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorProfileDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var success = await _doctorService.UpdateDoctorProfileAsync(userId, request);
        if (!success)
            return BadRequest("Lỗi khi cập nhật hồ sơ Bác sĩ.");

        return Ok(new { Message = "Cập nhật hồ sơ Bác sĩ thành công" });
    }

    // ==================== APPOINTMENTS ====================
    
    [HttpGet("appointments/today")]
    public async Task<IActionResult> GetAppointmentsToday()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var appointments = await _doctorService.GetAppointmentsTodayAsync(userId);
        return Ok(appointments);
    }

    [HttpGet("appointments/debug")]
    public async Task<IActionResult> GetAppointmentsDebug()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var appointments = await _doctorService.GetAppointmentsTodayAsync(userId);
        
        // Debug info
        var debugInfo = new
        {
            UserId = userId,
            Today = DateTime.Now.Date,
            Tomorrow = DateTime.Now.Date.AddDays(1),
            AppointmentCount = appointments.Count,
            Appointments = appointments
        };
        
        return Ok(debugInfo);
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAllAppointments()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var appointments = await _doctorService.GetAllAppointmentsAsync(userId);
        return Ok(appointments);
    }

    [HttpGet("appointments/date/{date}")]
    public async Task<IActionResult> GetAppointmentsByDate(DateTime date)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var appointments = await _doctorService.GetAppointmentsByDateAsync(userId, date);
        return Ok(appointments);
    }

    [HttpGet("appointments/{id}")]
    public async Task<IActionResult> GetAppointmentDetail(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var appointment = await _doctorService.GetAppointmentDetailAsync(userId, id);
        if (appointment == null)
            return NotFound("Không tìm thấy lịch hẹn.");

        return Ok(appointment);
    }

    [HttpPut("appointments/{id}/status")]
    public async Task<IActionResult> UpdateAppointmentStatus(Guid id, [FromBody] int status)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var success = await _doctorService.UpdateAppointmentStatusAsync(userId, id, status);
        if (!success)
            return BadRequest("Cập nhật trạng thái thất bại.");

        return Ok(new { Message = "Cập nhật trạng thái thành công" });
    }

    // ==================== MEDICAL RECORDS ====================
    
    [HttpGet("medical-records")]
    public async Task<IActionResult> GetMedicalRecords()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var records = await _doctorService.GetMedicalRecordsByDoctorAsync(userId);
        return Ok(records);
    }

    [HttpGet("medical-records/{id}")]
    public async Task<IActionResult> GetMedicalRecordDetail(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var record = await _doctorService.GetMedicalRecordDetailAsync(userId, id);
        if (record == null)
            return NotFound("Không tìm thấy bệnh án.");

        return Ok(record);
    }

    [HttpPost("medical-records")]
    public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var record = await _doctorService.CreateMedicalRecordAsync(userId, request);
        if (record == null)
            return BadRequest("Tạo bệnh án thất bại.");

        return Created($"/api/doctor/medical-records/{record.Id}", record);
    }

    [HttpPut("medical-records/{id}")]
    public async Task<IActionResult> UpdateMedicalRecord(Guid id, [FromBody] UpdateMedicalRecordDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var success = await _doctorService.UpdateMedicalRecordAsync(userId, id, request);
        if (!success)
            return BadRequest("Cập nhật bệnh án thất bại.");

        return Ok(new { Message = "Cập nhật bệnh án thành công" });
    }

    // ==================== PRESCRIPTIONS ====================
    
    [HttpGet("prescriptions/medical-record/{medicalRecordId}")]
    public async Task<IActionResult> GetPrescriptionByMedicalRecord(Guid medicalRecordId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var prescription = await _doctorService.GetPrescriptionByMedicalRecordAsync(userId, medicalRecordId);
        if (prescription == null)
            return NotFound("Không tìm thấy đơn thuốc.");

        return Ok(prescription);
    }

    [HttpPost("prescriptions")]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();

        var prescription = await _doctorService.CreatePrescriptionAsync(userId, request);
        if (prescription == null)
            return BadRequest("Tạo đơn thuốc thất bại.");

        return Created($"/api/doctor/prescriptions/medical-record/{prescription.MedicalRecordId}", prescription);
    }

    // ==================== MEDICATIONS ====================
    
    [HttpGet("medications")]
    public async Task<IActionResult> GetAllMedications()
    {
        var medications = await _doctorService.GetAllMedicationsAsync();
        return Ok(medications);
    }
}
