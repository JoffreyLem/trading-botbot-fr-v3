using System.Text.Json;
using System.Text.RegularExpressions;
using RobotAppLibrary.Api.Connector.Exceptions;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using Serilog;

namespace RobotAppLibrary.Api.Connector.Tcp;

public interface ITcpConnector : IConnectorBase
{
    Task<JsonDocument?> SendAndReceiveAsync(string messageToSend, bool logResponse = true);
}

public class TcpConnector(Server server, ILogger logger)
    : TcpClientBase(server.Address, server.MainPort, logger), ITcpConnector
{
    private static readonly Regex PasswordRegex = new("\"password\":\".*?\"", RegexOptions.Compiled);
    private static readonly Regex ApiKeyRegex = new("\"ApiKey\":\".*?\"", RegexOptions.Compiled);
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private long _lastCommandTimestamp;

    public async Task<JsonDocument?> SendAndReceiveAsync(string messageToSend, bool logResponse = true)
    {
        await _semaphore.WaitAsync();
        var tcpLog = new TcpLog
        {
            RequestMessage = messageToSend
        };
        try
        {
            var currentTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var interval = currentTimestamp - _lastCommandTimestamp;

            if (interval < CommandTimeSpanmeSpace.Ticks) await Task.Delay(CommandTimeSpanmeSpace).ConfigureAwait(false);

            await SendAsync(messageToSend).ConfigureAwait(false);
            _lastCommandTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var response =  await ReceiveAsync();
            tcpLog.ResponseMessage = logResponse ? response : null;

            return response;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error on send and receive");
            throw new ApiCommunicationException("Error on API Communication", e);
        }
        finally
        {
            Logger.Information("Tcp log received : {@Tcp}", tcpLog);
            _semaphore.Release();
        }
    }

}