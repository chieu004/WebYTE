using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Admin;
using WebYTE.Application.DTOs.Doctor;
using WebYTE.Application.DTOs.Medication;
using WebYTE.Application.DTOs.Patient;
using WebYTE.Application.DTOs.Staff;
using WebYTE.Application.Interfaces;

namespace WebYTE.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] // Root Access
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ==================== USER MANAGEMENT ====================
    
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _adminService.GetAllUsersAsync());
    }

    [HttpPut("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(Guid id)
    {
        var success = await _adminService.ToggleUserStatusAsync(id);
        return success ? Ok(new { Message = "Cập nhật trạng thái thành công" }) : BadRequest("Không tìm thấy User.");
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        return Ok(await _adminService.GetUserStatisticsAsync());
    }

    // ==================== SPECIALTY MANAGEMENT ====================
    
    [HttpGet("specialties")]
    [AllowAnonymous] // Cho phép ai cũng xem danh sách chuyên khoa
    public async Task<IActionResult> GetSpecialties()
    {
        return Ok(await _adminService.GetAllSpecialtiesAsync());
    }

    [HttpPost("specialties")]
    public async Task<IActionResult> CreateSpecialty([FromBody] SpecialtyDto request)
    {
        var result = await _adminService.CreateSpecialtyAsync(request);
        return Created("", result);
    }

    // ==================== DOCTOR MANAGEMENT ====================
    
    [HttpPost("doctors/create")]
    public async Task<IActionResult> CreateDoctorAccount([FromBody] CreateDoctorAccountDto request)
    {
        var (success, message) = await _adminService.CreateDoctorAccountAsync(request);
        return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetAllDoctors()
    {
        return Ok(await _adminService.GetAllDoctorsAsync());
    }

    [HttpGet("doctors/{id}")]
    public async Task<IActionResult> GetDoctorById(Guid id)
    {
        var doctor = await _adminService.GetDoctorByIdAsync(id);
        if (doctor == null)
            return NotFound("Không tìm thấy bác sĩ.");
        return Ok(doctor);
    }

    [HttpPut("doctors/{id}")]
    public async Task<IActionResult> UpdateDoctor(Guid id, [FromBody] UpdateDoctorProfileDto request)
    {
        var success = await _adminService.UpdateDoctorAsync(id, request);
        if (!success)
            return BadRequest("Cập nhật thất bại.");
        return Ok(new { Message = "Cập nhật hồ sơ bác sĩ thành công" });
    }

    [HttpDelete("doctors/{id}")]
    public async Task<IActionResult> DeleteDoctor(Guid id)
    {
        var success = await _adminService.DeleteDoctorAsync(id);
        if (!success)
            return BadRequest("Xóa thất bại.");
        return Ok(new { Message = "Xóa bác sĩ thành công" });
    }

    // ==================== PATIENT MANAGEMENT ====================
    
    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatients()
    {
        return Ok(await _adminService.GetAllPatientsAsync());
    }

    [HttpGet("patients/{id}")]
    public async Task<IActionResult> GetPatientById(Guid id)
    {
        var patient = await _adminService.GetPatientByIdAsync(id);
        if (patient == null)
            return NotFound("Không tìm thấy bệnh nhân.");
        return Ok(patient);
    }

    [HttpPut("patients/{id}")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientProfileDto request)
    {
        var success = await _adminService.UpdatePatientAsync(id, request);
        if (!success)
            return BadRequest("Cập nhật thất bại.");
        return Ok(new { Message = "Cập nhật hồ sơ bệnh nhân thành công" });
    }

    [HttpDelete("patients/{id}")]
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        var success = await _adminService.DeletePatientAsync(id);
        if (!success)
            return BadRequest("Xóa thất bại.");
        return Ok(new { Message = "Xóa bệnh nhân thành công" });
    }

    // ==================== STAFF MANAGEMENT ====================
    
    [HttpPost("staffs/create")]
    public async Task<IActionResult> CreateStaffAccount([FromBody] CreateStaffAccountDto request)
    {
        var (success, message) = await _adminService.CreateStaffAccountAsync(request);
        return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
    }

    [HttpGet("staffs")]
    public async Task<IActionResult> GetAllStaffs()
    {
        return Ok(await _adminService.GetAllStaffsAsync());
    }

    [HttpGet("staffs/{id}")]
    public async Task<IActionResult> GetStaffById(Guid id)
    {
        var staff = await _adminService.GetStaffByIdAsync(id);
        if (staff == null)
            return NotFound("Không tìm thấy nhân viên.");
        return Ok(staff);
    }

    [HttpPut("staffs/{id}")]
    public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateStaffProfileDto request)
    {
        var success = await _adminService.UpdateStaffAsync(id, request);
        if (!success)
            return BadRequest("Cập nhật thất bại.");
        return Ok(new { Message = "Cập nhật hồ sơ nhân viên thành công" });
    }

    [HttpDelete("staffs/{id}")]
    public async Task<IActionResult> DeleteStaff(Guid id)
    {
        var success = await _adminService.DeleteStaffAsync(id);
        if (!success)
            return BadRequest("Xóa thất bại.");
        return Ok(new { Message = "Xóa nhân viên thành công" });
    }

    // ==================== MEDICATION MANAGEMENT ====================

    [HttpGet("medications")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMedications()
    {
        return Ok(await _adminService.GetAllMedicationsAsync());
    }

    [HttpPost("medications")]
    public async Task<IActionResult> CreateMedication([FromBody] MedicationDto request)
    {
        var result = await _adminService.CreateMedicationAsync(request);
        return Created("", result);
    }

    [HttpPut("medications/{id}")]
    public async Task<IActionResult> UpdateMedication(Guid id, [FromBody] MedicationDto request)
    {
        var success = await _adminService.UpdateMedicationAsync(id, request);
        return success ? Ok(new { Message = "Cập nhật thuốc thành công" }) : BadRequest("Cập nhật thất bại.");
    }

    [HttpDelete("medications/{id}")]
    public async Task<IActionResult> DeleteMedication(Guid id)
    {
        var success = await _adminService.DeleteMedicationAsync(id);
        return success ? Ok(new { Message = "Xóa thuốc thành công" }) : BadRequest("Xóa thất bại.");
    }
}
