using Destructurama.Attributed;

namespace RobotAppLibrary.Api.Modeles;

public class Credentials
{
    public string? User { get; set; }

    [LogMasked] public string? Password { get; set; }

    [LogMasked] public string? ApiKey { get; set; }
}