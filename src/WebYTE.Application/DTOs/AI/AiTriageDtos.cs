using System.Collections.Generic;

namespace WebYTE.Application.DTOs.AI;

public class AiTriageRequestDto
{
    public string Symptoms { get; set; } = string.Empty;
}

public class AiTriageResponseDto
{
    public string TriageResult { get; set; } = string.Empty;
    public string SuggestedSpecialty { get; set; } = string.Empty;
    public bool RequiresImmediateAttention { get; set; }
}

// DTO support mapping request qua cấu trúc JSON chuẩn của LM Studio / OpenAI
public class OpenAiChatRequest
{
    public string model { get; set; } = "local-model";
    public List<OpenAiMessage> messages { get; set; } = new();
    public double temperature { get; set; } = 0.3; // Thấp để tư vấn Y Tế cẩn thận hơn
    public int max_tokens { get; set; } = 500; // Giới hạn độ dài response
}

public class OpenAiMessage
{
    public string role { get; set; } = "user"; // system, user, assistant
    public string content { get; set; } = string.Empty;
}

public class OpenAiChatResponse
{
    public List<OpenAiChoice> choices { get; set; } = new();
}

public class OpenAiChoice
{
    public OpenAiMessage message { get; set; } = new();
}
