using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebYTE.Application.DTOs.AI;
using WebYTE.Application.Interfaces;

namespace WebYTE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiDiagnosisController : ControllerBase
{
    private readonly IAiDiagnosisService _aiDiagnosisService;

    public AiDiagnosisController(IAiDiagnosisService aiDiagnosisService)
    {
        _aiDiagnosisService = aiDiagnosisService;
    }

    [HttpPost("diagnose")]
    [AllowAnonymous] // Allow anyone to use AI diagnosis
    public async Task<IActionResult> Diagnose([FromBody] AiDiagnosisRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Symptoms))
        {
            return BadRequest(new { message = "Vui lòng nhập triệu chứng" });
        }

        var result = await _aiDiagnosisService.DiagnoseAsync(request);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
