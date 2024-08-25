using System.Text.Json;
using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Executor;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers.Xtb.Modeles;
using Serilog;

namespace RobotAppLibrary.Api.Providers.Xtb;

public class XtbCommandExecutor(
    TcpConnector tcpClient,
    StreamingClientXtb tcpStreamingClient,
    CommandCreatorXtb commandCreator,
    XtbAdapter responseAdapter,
    ILogger logger)
    : TcpCommandExecutor(tcpClient, tcpStreamingClient,
        commandCreator, responseAdapter, logger)
{
    public override async Task ExecuteLoginCommand(Credentials credentials)
    {
        Logger.Information("Execute login command for user {User}", credentials.User);
        await TcpClient.ConnectAsync();
        var command = CommandCreator.CreateLoginCommand(credentials);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        var rspAdapter = ResponseAdapter.AdaptLoginResponse(rsp);
        ((CommandCreatorXtb)CommandCreator).StreamingSessionId = ((LoginResponseXtb)rspAdapter).StreamingSessionId;
        await TcpStreamingClient.ConnectAsync();
    }
}