using System.Text.Json.Serialization;

namespace RobotAppLibrary.LLM.Repositories.Gemini.Dao.Request;

internal class GeminiGenerationRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = new List<GeminiContent>();
    
    [JsonPropertyName("generationConfig")]
    public GenerationConfig GenerationConfig { get; set; } = new GenerationConfig();
}