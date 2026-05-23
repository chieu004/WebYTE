using System.Threading.Tasks;
using WebYTE.Application.DTOs.AI;

namespace WebYTE.Application.Interfaces;

public interface IAiTriageService
{
    Task<AiTriageResponseDto> AnalyzeSymptomsAsync(AiTriageRequestDto request);
}
