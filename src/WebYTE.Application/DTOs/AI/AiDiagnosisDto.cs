namespace WebYTE.Application.DTOs.AI;

public class AiDiagnosisRequest
{
    public string Symptoms { get; set; } = string.Empty;
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
}

public class AiDiagnosisResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public DiagnosisResult? Result { get; set; }
}

public class DiagnosisResult
{
    public List<PossibleCondition> PossibleConditions { get; set; } = new();
    public List<string> RecommendedTests { get; set; } = new();
    public List<string> GeneralAdvice { get; set; } = new();
    public string Disclaimer { get; set; } = "Đây chỉ là kết quả tham khảo từ AI. Vui lòng đến gặp bác sĩ để được chẩn đoán chính xác.";
    public int SeverityLevel { get; set; } // 1-5: 1=Nhẹ, 5=Nghiêm trọng
}

public class PossibleCondition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Probability { get; set; } // 0-100%
    public List<string> CommonSymptoms { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}
