using System.Text.Json.Serialization;

namespace RobotAppLibrary.LLM.Repositories.Gemini.Dao.Request;

internal class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}