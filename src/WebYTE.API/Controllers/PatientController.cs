using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Patient;
using WebYTE.Application.Interfaces;

namespace WebYTE.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Patient")] // Gắn cờ bắt buộc là Bệnh Nhân
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        var profile = await _patientService.GetPatientProfileAsync(userId);
        if (profile == null)
            return NotFound("Không tìm thấy thông tin bệnh nhân.");

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePatientProfileDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }

        var success = await _patientService.UpdatePatientProfileAsync(userId, request);
        if (!success)
            return BadRequest("Lỗi khi cập nhật thông tin.");

        return Ok(new { Message = "Cập nhật hồ sơ thành công" });
    }

    [HttpGet("doctors")]
    [AllowAnonymous] // Bất cứ Bệnh nhân nào cũng thấy để chọn
    public async Task<IActionResult> GetDoctors()
    {
        return Ok(await _patientService.GetDoctorsAsync());
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var success = await _patientService.BookAppointmentAsync(userId, request);
        return success ? Ok() : BadRequest("Không thể tạo lịch hẹn.");
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var appointments = await _patientService.GetMyAppointmentsAsync(userId);
        return Ok(appointments);
    }
}
