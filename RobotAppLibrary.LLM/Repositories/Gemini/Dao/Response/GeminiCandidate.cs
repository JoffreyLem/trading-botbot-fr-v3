using System.Text.Json.Serialization;
using RobotAppLibrary.LLM.Repositories.Gemini.Dao.Request;

namespace RobotAppLibrary.LLM.Repositories.Gemini.Dao.Response;

internal class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}