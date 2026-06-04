using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebYTE.Application.Interfaces;
using WebYTE.Core.Enums;

namespace WebYTE.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost("generate/{appointmentId}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GenerateInvoice(Guid appointmentId)
    {
        try
        {
            var invoiceId = await _invoiceService.GenerateInvoiceAsync(appointmentId);
            return Ok(invoiceId);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{invoiceId}")]
    public async Task<IActionResult> GetInvoice(Guid invoiceId)
    {
        var invoice = await _invoiceService.GetInvoiceDetailsAsync(invoiceId);
        if (invoice == null)
            return NotFound(new { message = "Không tìm thấy hóa đơn" });

        return Ok(invoice);
    }

    [HttpPut("{invoiceId}/status")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> UpdateInvoiceStatus(Guid invoiceId, [FromBody] UpdateInvoiceStatusRequest request)
    {
        try
        {
            var success = await _invoiceService.UpdateInvoiceStatusAsync(invoiceId, (InvoiceStatus)request.Status);
            if (success)
                return Ok(new { message = "Cập nhật trạng thái hóa đơn thành công" });
            
            return BadRequest(new { message = "Không tìm thấy hóa đơn" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class UpdateInvoiceStatusRequest
{
    public int Status { get; set; }
}
