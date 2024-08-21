using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using RobotAppLibrary.Api.Connector.Exceptions;
using RobotAppLibrary.Api.Interfaces;
using Serilog;

namespace RobotAppLibrary.Api.Connector.Tcp;

public abstract class TcpClientBase : IConnectorBase, IDisposable
{
    private readonly TcpClient _client = new();

    private readonly int _port;

    private readonly string _serverAddress;

    private readonly TimeSpan _timeOutMilliSeconds = TimeSpan.FromMilliseconds(5000);

    protected readonly ILogger Logger;

    private StreamReader? _apiReadStream;

    private StreamWriter? _apiWriteStream;

    private SslStream? _stream;

    protected TimeSpan CommandTimeSpanmeSpace = TimeSpan.FromMilliseconds(200);

    protected TcpClientBase(string serverAddress, int port, ILogger logger)
    {
        Logger = logger.ForContext(GetType());
        _serverAddress = serverAddress;
        _port = port;
    }

    public bool IsConnected => _client.Connected;

    public void Dispose()
    {
        _apiReadStream?.Dispose();
        _apiWriteStream?.Dispose();
        _client.Dispose();
    }

    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public virtual async Task ConnectAsync()
    {
        try
        {
            if (IsConnected) return;
            var connectTask = _client.ConnectAsync(_serverAddress, _port);
            var delayTask = Task.Delay(_timeOutMilliSeconds);

            var completedTask = await Task.WhenAny(connectTask, delayTask);

            if (completedTask == delayTask)
            {
                Close();
                throw new ApiCommunicationException("Connection timed out.");
            }

            _stream = new SslStream(_client.GetStream(), false, ValidateServerCertificate);
            var authenticationTask = _stream.AuthenticateAsClientAsync(_serverAddress, new X509CertificateCollection(),
                SslProtocols.Tls13 | SslProtocols.Tls12, true);
            var delayTask2 = Task.Delay(TimeSpan.FromSeconds(30));

            var completedTask2 = await Task.WhenAny(authenticationTask, delayTask2);

            if (completedTask2 == delayTask) throw new TimeoutException("SSL handshake timed out.");

            _apiWriteStream ??= new StreamWriter(_stream, leaveOpen: false);
            _apiReadStream ??= new StreamReader(_stream, leaveOpen: false);
            OnConnectedEvent();
        }
        catch (Exception e)
        {
            Logger.Information(e, "Error on tcp connection");
            Close();
            throw new ApiCommunicationException("Error on connection", e);
        }
    }

    public virtual async Task SendAsync(string messageToSend)
    {
        if (!IsConnected)
        {
            Close();
            throw new ApiCommunicationException("Error while sending the data (socket disconnected)");
        }

        try
        {
            await _apiWriteStream!.WriteAsync(messageToSend);
            await _apiWriteStream.FlushAsync();
        }
        catch (IOException ex)
        {
            Close();
            throw new ApiCommunicationException("Error while sending the data: " + ex.Message);
        }
    }

    public async Task<JsonDocument?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        var pipeReader = PipeReader.Create(_stream);
    
            var delimiter = "\n\n"u8.ToArray();
         
            while (true)
            {
                ReadResult result = await pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (true)
                {
        
                    var position = buffer.PositionOf((byte)'\n');

                    if (position != null)
                    {
                        try
                        {
                            var data=JsonDocument.Parse(buffer);
                            var next = buffer.GetPosition(delimiter.Length, position.Value);
                            pipeReader.AdvanceTo(next);
                            return data;
                        }
                        catch (Exception e)
                        {
                            string bufferText = Encoding.UTF8.GetString(buffer.ToArray());
                            Logger.Error(e, $"An error occurred on response receive TCP. Buffer content: {bufferText}");
                            await pipeReader.CompleteAsync().ConfigureAwait(false);
                            return null;
                        }
                    }

                    break;
                }

                pipeReader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
            await pipeReader.CompleteAsync().ConfigureAwait(false);
            return null;
      
    }
    public void Close()
    {
        if (IsConnected)
        {
            _apiReadStream?.Close();
            _apiWriteStream?.Close();
            _client.Close();
            OnDisconnected();
        }
    }

    private void OnConnectedEvent()
    {
        Logger.Information("{Connector} Connected to {server}:{port}", GetType().Name, _serverAddress, _port);
        Connected?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisconnected()
    {
        Logger.Information("{Connector} Disconnected from {server}:{port}", GetType().Name, _serverAddress, _port);
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    protected virtual bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        Logger.Error("Certificate error: {0}", sslPolicyErrors);

        return false;
    }
}