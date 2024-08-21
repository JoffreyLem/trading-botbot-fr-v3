using System.Text.Json;
using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Executor;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers.Xtb.Modeles;

namespace RobotAppLibrary.Api.Providers.Xtb;

public class XtbCommandExecutor(
    TcpConnector tcpClient,
    StreamingClientXtb tcpStreamingClient,
    CommandCreatorXtb commandCreator,
    XtbAdapter responseAdapter)
    : TcpCommandExecutor(tcpClient, tcpStreamingClient,
        commandCreator, responseAdapter)
{
    public override async Task ExecuteLoginCommand(Credentials credentials)
    {
        await TcpClient.ConnectAsync();
        var command = CommandCreator.CreateLoginCommand(credentials);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        var rspAdapter = ResponseAdapter.AdaptLoginResponse(rsp);
        ((CommandCreatorXtb)CommandCreator).StreamingSessionId = ((LoginResponseXtb)rspAdapter).StreamingSessionId;
        await TcpStreamingClient.ConnectAsync();
    }
}