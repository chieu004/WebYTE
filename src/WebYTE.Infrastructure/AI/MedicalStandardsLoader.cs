using System.Text;
using System.Text.Json;

namespace WebYTE.Infrastructure.AI;

/// <summary>
/// Loads medical standards from JSON and formats a compact system prompt context.
/// Only critical rules and emergency flags are injected — full JSON is NOT dumped into the prompt.
/// This keeps token count low (~300-400 tokens) to avoid slowing down the local AI model.
/// </summary>
public static class MedicalStandardsLoader
{
    private static string? _cachedPrompt;
    private const int MaxWarningSignLength = 80; // Truncate long warning signs

    public static string LoadAsPromptContext(string jsonFilePath)
    {
        if (_cachedPrompt != null) return _cachedPrompt;

        try
        {
            if (!File.Exists(jsonFilePath))
            {
                _cachedPrompt = GetFallbackContext();
                return _cachedPrompt;
            }

            var json = File.ReadAllText(jsonFilePath, Encoding.UTF8);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var sb = new StringBuilder();
            sb.AppendLine("=== TIÊU CHUẨN Y TẾ VIỆT NAM (Phiên bản " + GetString(root, "version") + ") ===");
            sb.AppendLine();

            // General rules
            sb.AppendLine("QUY TẮC BẮT BUỘC:");
            foreach (var rule in root.GetProperty("generalRules").EnumerateArray())
                sb.AppendLine($"- {rule.GetString()}");
            sb.AppendLine();

            // Prohibited actions
            sb.AppendLine("HÀNH ĐỘNG BỊ CẤM:");
            foreach (var item in root.GetProperty("prohibitedActions").EnumerateArray())
                sb.AppendLine($"- {item.GetString()}");
            sb.AppendLine();

            // Emergency symptoms
            sb.AppendLine("TRIỆU CHỨNG CẤP CỨU (yêu cầu gọi 115 ngay):");
            foreach (var sym in root.GetProperty("emergencySymptoms").EnumerateArray())
                sb.AppendLine($"- {sym.GetString()}");
            sb.AppendLine();

            // Diagnostic guidelines — chỉ lấy warning sign, truncate nếu quá dài
            sb.AppendLine("DẤU HIỆU CẢNH BÁO THEO CHUYÊN KHOA:");
            foreach (var dept in root.GetProperty("diagnosticGuidelines").EnumerateObject())
            {
                var d = dept.Value;
                var name = GetString(d, "name");
                var warn = GetString(d, "warningSign");
                if (warn.Length > MaxWarningSignLength)
                    warn = warn.Substring(0, MaxWarningSignLength) + "...";
                sb.AppendLine($"[{name}]: {warn}");
            }
            sb.AppendLine();

            // Disclaimer
            sb.AppendLine("TUYÊN BỐ MIỄN TRỪ TRÁCH NHIỆM:");
            sb.AppendLine(GetString(root, "disclaimer"));

            _cachedPrompt = sb.ToString();
            return _cachedPrompt;
        }
        catch
        {
            _cachedPrompt = GetFallbackContext();
            return _cachedPrompt;
        }
    }

    private static string GetString(JsonElement el, string prop)
    {
        return el.TryGetProperty(prop, out var val) ? val.GetString() ?? "" : "";
    }

    private static string GetFallbackContext()
    {
        return @"QUY TẮC BẮT BUỘC:
- Không chẩn đoán xác định bệnh, chỉ đưa ra khả năng tham khảo
- Không kê đơn thuốc cụ thể với liều lượng
- Luôn khuyến nghị bệnh nhân đến cơ sở y tế
- Với triệu chứng nguy hiểm tính mạng, yêu cầu gọi cấp cứu 115 ngay";
    }
}
