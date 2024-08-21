using System.Text.Json;

namespace RobotAppLibrary.Api.Modeles;

public class TcpLog
{
    public string? RequestMessage { get; set; }

    public JsonDocument? ResponseMessage { get; set; }
}