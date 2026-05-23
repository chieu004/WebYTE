using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.AI;
using WebYTE.Application.Interfaces;

namespace WebYTE.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Yêu cầu đăng nhập (cả bệnh nhân, staff hoặc admin)
public class AiTriageController : ControllerBase
{
    private readonly IAiTriageService _aiService;

    public AiTriageController(IAiTriageService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeSymptoms([FromBody] AiTriageRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Symptoms))
        {
            return BadRequest("Vui lòng cung cấp triệu chứng của bạn.");
        }

        var result = await _aiService.AnalyzeSymptomsAsync(request);
        return Ok(result);
    }
}
