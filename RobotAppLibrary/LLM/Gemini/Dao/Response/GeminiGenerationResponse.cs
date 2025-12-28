using System.Text.Json.Serialization;

namespace RobotAppLibrary.LLM.Gemini.Dao.Response;

internal class GeminiGenerationResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}