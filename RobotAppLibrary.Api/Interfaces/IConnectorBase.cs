using System.Text.Json;

namespace RobotAppLibrary.Api.Interfaces;

public interface IConnectorBase : IConnectionEvent
{
    bool IsConnected { get; }
    void Dispose();
    Task ConnectAsync();
    Task SendAsync(string messageToSend);
    Task<JsonDocument?> ReceiveAsync(CancellationToken cancellationToken = default);
    void Close();
}