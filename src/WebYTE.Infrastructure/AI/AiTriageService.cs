using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WebYTE.Application.DTOs.AI;
using WebYTE.Application.Interfaces;

namespace WebYTE.Infrastructure.AI;

public class AiTriageService : IAiTriageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _standardsFilePath;

    public AiTriageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        var baseUrl = _configuration["LMStudio:BaseUrl"] ?? "http://localhost:1234/v1/";
        _httpClient.BaseAddress = new Uri(baseUrl);

        // Tìm file JSON tiêu chuẩn y tế từ thư mục chạy
        var basePath = _configuration["MedicalStandards:Path"] ?? AppContext.BaseDirectory;
        _standardsFilePath = Path.Combine(basePath, "medical-standards.json");
    }

    public async Task<AiTriageResponseDto> AnalyzeSymptomsAsync(AiTriageRequestDto request)
    {
        // Load tiêu chuẩn y tế từ file JSON
        var medicalStandards = MedicalStandardsLoader.LoadAsPromptContext(_standardsFilePath);

        var systemPrompt = $@"Bạn là một trợ lý y khoa AI của phòng khám WebYTE.

{medicalStandards}

=== NHIỆM VỤ ===
Phân tích sơ bộ triệu chứng bệnh nhân, gợi ý chuyên khoa phù hợp và mức độ ưu tiên khám.
Từ chối trả lời các câu hỏi không liên quan đến y tế.
Hãy tóm tắt và đưa ra chuyên khoa phù hợp bằng tiếng Việt.";

        var chatRequest = new OpenAiChatRequest
        {
            model = _configuration["LMStudio:Model"] ?? "local-model",
            temperature = 0.3, // Low temp for more deterministic output
            max_tokens = 300, // Giới hạn độ dài response để nhanh hơn
            messages = new List<OpenAiMessage>
            {
                new OpenAiMessage { role = "system", content = systemPrompt },
                new OpenAiMessage { role = "user", content = $"Triệu chứng của tôi: {request.Symptoms}" }
            }
        };

        try
        {
            // Call LM Studio (OpenAI compatible endpoint)
            var response = await _httpClient.PostAsJsonAsync("chat/completions", chatRequest);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
            var aiText = result?.choices?[0]?.message?.content ?? "Không thể phân tích triệu chứng lúc này.";

            // Parsing logic cơ bản (Có thể dùng prompt Regex hoặc cấu trúc JSON Mode nếu model hỗ trợ)
            bool requiresImmediate = aiText.Contains("cấp cứu") || aiText.Contains("ngay lập tức") || aiText.Contains("nguy hiểm");
            string suggestedSpecialty = "Đa khoa";
            if (aiText.Contains("đau tim") || aiText.Contains("tim mạch")) suggestedSpecialty = "Tim mạch";
            else if (aiText.Contains("đau bụng") || aiText.Contains("dạ dày")) suggestedSpecialty = "Tiêu hóa";
            else if (aiText.Contains("con nít") || aiText.Contains("trẻ em")) suggestedSpecialty = "Nhi khoa";

            return new AiTriageResponseDto
            {
                TriageResult = aiText,
                SuggestedSpecialty = suggestedSpecialty,
                RequiresImmediateAttention = requiresImmediate
            };
        }
        catch (HttpRequestException ex)
        {
            // Fallback khi LM Studio chưa được bật
            return new AiTriageResponseDto
            {
                TriageResult = $"Không thể kết nối đến AI. Lỗi mạng: Vui lòng kiểm tra LM Studio (Localhost:1234). Chi tiết: {ex.Message}",
                SuggestedSpecialty = "Không xác định",
                RequiresImmediateAttention = false
            };
        }
    }
}
