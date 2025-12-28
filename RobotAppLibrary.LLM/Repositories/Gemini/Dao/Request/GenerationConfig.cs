using System.Text.Json.Serialization;

namespace RobotAppLibrary.LLM.Repositories.Gemini.Dao.Request;

public class GenerationConfig
{
    [JsonPropertyName("response_mime_type")]
    public string ResponseMimeType { get; set; }
}