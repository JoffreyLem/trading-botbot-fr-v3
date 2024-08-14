using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Api.Providers.Xtb;
using Serilog;

namespace RobotAppLibrary.Api.Providers;

public static class ApiProviderFactory
{
    public static IApiProviderBase GetApiHandler(ApiProviderEnum api, ILogger logger)
    {
        return api switch
        {
            ApiProviderEnum.Xtb => GetXtbApiHandler(logger),
            _ => throw new ArgumentException($"{api.ToString()} not handled")
        };
    }

    private static IApiProviderBase GetXtbApiHandler(ILogger logger)
    {
        var tcpConnector = new TcpConnector(XtbServer.DemoTcp, logger);
        var adapter = new XtbAdapter();
        var streamingCLient = new StreamingClientXtb(XtbServer.DemoTcp, logger, adapter);
        var commandCreator = new CommandCreatorXtb();
        var icommandExecutor = new XtbCommandExecutor(tcpConnector, streamingCLient, commandCreator, adapter);
        return new XtbApiProvider(icommandExecutor, logger, TimeSpan.FromMinutes(10));
    }
}