namespace RobotAppLibrary.Api.Interfaces;

public interface IConnectorBase : IConnectionEvent
{
    void Dispose();
    Task ConnectAsync();
    Task SendAsync(string messageToSend);
    Task<string> ReceiveAsync(CancellationToken cancellationToken = default);
    void Close();
     bool IsConnected { get;  }
}