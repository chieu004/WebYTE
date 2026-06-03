using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using WebYTE.Application.DTOs.AI;
using WebYTE.Application.Interfaces;

namespace WebYTE.Infrastructure.AI;

public class AiDiagnosisService : IAiDiagnosisService
{
    private readonly HttpClient _httpClient;
    private readonly string _standardsFilePath;
    private const string LM_STUDIO_URL = "http://localhost:1234/v1/chat/completions";

    public AiDiagnosisService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var basePath = configuration["MedicalStandards:Path"] ?? AppContext.BaseDirectory;
        _standardsFilePath = Path.Combine(basePath, "medical-standards.json");
    }

    public async Task<AiDiagnosisResponse> DiagnoseAsync(AiDiagnosisRequest request)
    {
        try
        {
            // Build prompt for AI
            var prompt = BuildDiagnosisPrompt(request);

            // Call LM Studio API
            var aiRequest = new
            {
                model = "local-model",
                messages = new[]
                {
                    new { role = "system", content = GetSystemPrompt() },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 500 // Giảm từ 2000 xuống 500 để nhanh hơn
            };

            var response = await _httpClient.PostAsJsonAsync(LM_STUDIO_URL, aiRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                return new AiDiagnosisResponse
                {
                    IsSuccess = false,
                    Message = "Không thể kết nối đến AI service. Vui lòng kiểm tra LM Studio đang chạy."
                };
            }

            var aiResponse = await response.Content.ReadFromJsonAsync<LMStudioResponse>();
            
            if (aiResponse?.choices == null || aiResponse.choices.Length == 0)
            {
                return new AiDiagnosisResponse
                {
                    IsSuccess = false,
                    Message = "AI không trả về kết quả."
                };
            }

            var aiContent = aiResponse.choices[0].message.content;
            
            // Parse AI response
            var result = ParseAiResponse(aiContent);

            return new AiDiagnosisResponse
            {
                IsSuccess = true,
                Message = "Phân tích thành công",
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new AiDiagnosisResponse
            {
                IsSuccess = false,
                Message = $"Lỗi: {ex.Message}"
            };
        }
    }

    private string GetSystemPrompt()
    {
        // Load tiêu chuẩn y tế từ file JSON
        var medicalStandards = MedicalStandardsLoader.LoadAsPromptContext(_standardsFilePath);

        return $@"Bạn là một trợ lý y tế AI chuyên nghiệp của hệ thống WebYTE.

{medicalStandards}

=== NHIỆM VỤ ===
Phân tích triệu chứng bệnh nhân và đưa ra các khả năng bệnh lý có thể xảy ra.
- Đưa ra 2-4 khả năng bệnh lý phổ biến nhất
- Đánh giá mức độ nghiêm trọng từ 1-5
- Gợi ý xét nghiệm cần thiết theo tiêu chuẩn
- Đưa ra lời khuyên phù hợp

Trả lời theo định dạng JSON sau:
{{
  ""possibleConditions"": [
    {{
      ""name"": ""Tên bệnh"",
      ""description"": ""Mô tả ngắn gọn"",
      ""probability"": 70,
      ""commonSymptoms"": [""triệu chứng 1"", ""triệu chứng 2""],
      ""recommendation"": ""Khuyến nghị""
    }}
  ],
  ""recommendedTests"": [""Xét nghiệm 1"", ""Xét nghiệm 2""],
  ""generalAdvice"": [""Lời khuyên 1"", ""Lời khuyên 2""],
  ""severityLevel"": 3
}}";
    }

    private string BuildDiagnosisPrompt(AiDiagnosisRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Phân tích triệu chứng sau:");
        sb.AppendLine($"- Triệu chứng: {request.Symptoms}");
        sb.AppendLine($"- Tuổi: {request.Age}");
        sb.AppendLine($"- Giới tính: {request.Gender}");
        
        if (!string.IsNullOrEmpty(request.MedicalHistory))
        {
            sb.AppendLine($"- Tiền sử bệnh: {request.MedicalHistory}");
        }
        
        if (!string.IsNullOrEmpty(request.Allergies))
        {
            sb.AppendLine($"- Dị ứng: {request.Allergies}");
        }

        sb.AppendLine("\nHãy phân tích và trả về kết quả theo định dạng JSON đã yêu cầu.");
        
        return sb.ToString();
    }

    private DiagnosisResult ParseAiResponse(string aiContent)
    {
        try
        {
            // Try to extract JSON from response
            var jsonStart = aiContent.IndexOf('{');
            var jsonEnd = aiContent.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = aiContent.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var result = JsonSerializer.Deserialize<DiagnosisResult>(jsonContent, options);
                
                if (result != null)
                {
                    return result;
                }
            }

            // Fallback: Create a basic result from text
            return new DiagnosisResult
            {
                PossibleConditions = new List<PossibleCondition>
                {
                    new PossibleCondition
                    {
                        Name = "Cần đánh giá thêm",
                        Description = aiContent,
                        Probability = 50,
                        CommonSymptoms = new List<string>(),
                        Recommendation = "Vui lòng đến gặp bác sĩ để được khám và chẩn đoán chính xác."
                    }
                },
                RecommendedTests = new List<string> { "Khám lâm sàng tổng quát" },
                GeneralAdvice = new List<string> 
                { 
                    "Theo dõi triệu chứng",
                    "Ghi chép các thay đổi",
                    "Đến gặp bác sĩ nếu triệu chứng nặng hơn"
                },
                SeverityLevel = 3
            };
        }
        catch
        {
            return new DiagnosisResult
            {
                PossibleConditions = new List<PossibleCondition>
                {
                    new PossibleCondition
                    {
                        Name = "Không thể phân tích",
                        Description = "AI không thể phân tích triệu chứng. Vui lòng đến gặp bác sĩ.",
                        Probability = 0,
                        CommonSymptoms = new List<string>(),
                        Recommendation = "Đến gặp bác sĩ để được khám và chẩn đoán."
                    }
                },
                RecommendedTests = new List<string>(),
                GeneralAdvice = new List<string> { "Đến gặp bác sĩ" },
                SeverityLevel = 3
            };
        }
    }

    // LM Studio response model
    private class LMStudioResponse
    {
        public Choice[]? choices { get; set; }
    }

    private class Choice
    {
        public Message message { get; set; } = new();
    }

    private class Message
    {
        public string content { get; set; } = string.Empty;
    }
}
