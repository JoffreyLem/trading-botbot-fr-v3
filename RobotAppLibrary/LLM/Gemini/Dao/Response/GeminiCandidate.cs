using System.Text.Json.Serialization;
using RobotAppLibrary.LLM.Gemini.Dao.Request;

namespace RobotAppLibrary.LLM.Gemini.Dao.Response;

internal class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}