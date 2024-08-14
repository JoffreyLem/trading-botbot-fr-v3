using RobotAppLibrary.Api.Modeles;

namespace RobotAppLibrary.Api.Providers.Xtb.Modeles;

public class LoginResponseXtb : LoginResponse
{
    public virtual string? StreamingSessionId { get; set; }
}