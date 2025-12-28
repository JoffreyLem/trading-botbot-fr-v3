
using System.Text.Json.Serialization;

namespace RobotAppLibrary.LLM.Gemini.Dao.Request;

internal class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = new List<GeminiPart>();
}