using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Providers.Base;
using Serilog;

namespace RobotAppLibrary.Api.Providers.Xtb;

public class XtbApiProvider(ICommandExecutor commandExecutor, ILogger logger, TimeSpan pingInterval)
    : ApiProviderBase(commandExecutor, logger, pingInterval)
{
    public override ApiProviderEnum ApiProviderName => ApiProviderEnum.Xtb;
}