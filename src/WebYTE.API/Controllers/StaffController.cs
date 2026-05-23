using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.Staff;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Enums;

namespace WebYTE.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Staff,Admin")] // Staff và Admin được phép dùng API này
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var profile = await _staffService.GetProfileAsync(userId);
        if (profile == null) return NotFound("Không tìm thấy Nhân viên.");

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStaffProfileDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var success = await _staffService.UpdateProfileAsync(userId, request);
        return success ? Ok() : BadRequest("Cập nhật thất bại.");
    }

    [HttpGet("appointments/pending")]
    public async Task<IActionResult> GetPendingAppointments()
    {
        var appointments = await _staffService.GetAllPendingAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _staffService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpPut("appointments/{id}/status")]
    public async Task<IActionResult> UpdateAppointmentStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        Console.WriteLine($"[DEBUG] UpdateAppointmentStatus called - ID: {id}, Status: {request.Status}");
        
        var success = await _staffService.UpdateAppointmentStatusAsync(id, (AppointmentStatus)request.Status);
        
        Console.WriteLine($"[DEBUG] UpdateAppointmentStatus result: {success}");
        
        if (success)
        {
            return Ok(new { message = "Cập nhật thành công", appointmentId = id, newStatus = request.Status });
        }
        
        return BadRequest(new { message = "Xử lý thất bại - Không tìm thấy lịch hẹn", appointmentId = id });
    }

    [HttpPut("appointments/{id}/payment")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] UpdatePaymentRequest request)
    {
        var success = await _staffService.ProcessPaymentAsync(id, (PaymentStatus)request.PaymentStatus);
        return success ? Ok(new { message = "Cập nhật thành công" }) : BadRequest(new { message = "Xử lý thanh toán thất bại" });
    }
}

// Request DTOs
public class UpdateStatusRequest
{
    public int Status { get; set; }
}

public class UpdatePaymentRequest
{
    public int PaymentStatus { get; set; }
}
