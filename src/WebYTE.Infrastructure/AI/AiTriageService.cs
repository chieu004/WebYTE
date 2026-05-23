using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

    public AiTriageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Cấu hình URL mặc định nếu không có trong appsettings
        var baseUrl = _configuration["LMStudio:BaseUrl"] ?? "http://localhost:1234/v1/";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<AiTriageResponseDto> AnalyzeSymptomsAsync(AiTriageRequestDto request)
    {
        var systemPrompt = @"Bạn là một trợ lý y khoa AI của phòng khám WebYTE. 
Nhiệm vụ của bạn là chẩn đoán sơ bộ triệu chứng của bệnh nhân. 
Quy tắc: 
1. Chỉ phân tích dựa trên y học hiện đại.
2. Từ chối trả lời các câu hỏi không liên quan đến y tế.
3. Không được kê đơn thuốc cứng, chỉ đưa ra lời khuyên đi khám.
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
