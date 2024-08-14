using System.Text.RegularExpressions;
using RobotAppLibrary.Api.Connector.Exceptions;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using Serilog;

namespace RobotAppLibrary.Api.Connector.Tcp;

public interface ITcpConnector : IConnectorBase
{
    Task<string> SendAndReceiveAsync(string messageToSend, bool logResponse = true);
}

public class TcpConnector(Server server, ILogger logger) : TcpClientBase(server.Address, server.MainPort, logger), ITcpConnector
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private long _lastCommandTimestamp;

    public async Task<string> SendAndReceiveAsync(string messageToSend, bool logResponse = true)
    {
        await _semaphore.WaitAsync();
        var tcpLog = new TcpLog
        {
            RequestMessage = FilterSensitiveData(messageToSend)
        };
        try
        {
            var currentTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var interval = currentTimestamp - _lastCommandTimestamp;

            if (interval < CommandTimeSpanmeSpace.Ticks)
            {
                await Task.Delay(CommandTimeSpanmeSpace - TimeSpan.FromMilliseconds(interval));
            }

            await SendAsync(messageToSend);
            _lastCommandTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var response = await ReceiveAsync();
            tcpLog.ResponseMessage = logResponse ? FilterSensitiveData(response) : "Response not logged";

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

    private static readonly Regex PasswordRegex = new Regex("\"password\":\".*?\"", RegexOptions.Compiled);
    private static readonly Regex ApiKeyRegex = new Regex("\"ApiKey\":\".*?\"", RegexOptions.Compiled);

    private string FilterSensitiveData(string message)
    {
        message = PasswordRegex.Replace(message, "\"password\":\"****\"");
        message = ApiKeyRegex.Replace(message, "\"ApiKey\":\"****\"");
        return message;
    }

}