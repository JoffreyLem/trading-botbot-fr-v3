using System.Text.Json.Serialization;

namespace RobotAppLibrary.LLM.Gemini.Dao.Request;

internal class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}