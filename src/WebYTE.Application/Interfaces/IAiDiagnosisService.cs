using WebYTE.Application.DTOs.AI;

namespace WebYTE.Application.Interfaces;

public interface IAiDiagnosisService
{
    Task<AiDiagnosisResponse> DiagnoseAsync(AiDiagnosisRequest request);
}
