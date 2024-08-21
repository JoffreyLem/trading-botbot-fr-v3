using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RobotAppLibrary.Tests.Integrations.Api.Connector.Tcp.Mock;

public class TcpServerMock : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly X509Certificate2 _certificate;
    private readonly TcpListener _listener;
    private readonly Thread _listenerThread;
    private SslStream _sslStream;

    public TcpServerMock(int port, X509Certificate2 certificate)
    {
        _certificate = certificate;
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        _listenerThread = new Thread(ListenForClients);
        _listenerThread.Start();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _listenerThread.Join();
        _sslStream?.Dispose();
        _cancellationTokenSource.Dispose();
    }

    private void ListenForClients()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var client = _listener.AcceptTcpClient();
                var clientThread = new Thread(() => HandleClientAsync(client));
                clientThread.Start();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected exception on cancellation
        }
        finally
        {
            _listener.Stop();
        }
    }

    private async void HandleClientAsync(TcpClient client)
    {
        await using (_sslStream = new SslStream(client.GetStream(), false))
        {
            await _sslStream.AuthenticateAsServerAsync(_certificate, false, false);

            using var reader = new StreamReader(_sslStream, Encoding.UTF8);
            var buffer = new char[1024];
            var stringBuilder = new StringBuilder();

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested && client.Connected)
                    if (client.Available > 0)
                    {
                        var charsRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                        if (charsRead > 0)
                        {
                            stringBuilder.Append(buffer, 0, charsRead);
                            var incomingMessage = stringBuilder.ToString();
                            Console.WriteLine("Received: " + incomingMessage);

                            var jsonResponse = "{\"status\": \"Ok\"}\n";
                            await _sslStream.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse));


                            stringBuilder.Clear();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        await Task.Delay(100); // Sleep briefly to avoid busy-waiting
                    }
            }
            catch (IOException)
            {
                // Handle the case when the client forcibly closes the connection
            }
            finally
            {
                client.Close();
            }
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_sslStream != null) await _sslStream.WriteAsync(Encoding.UTF8.GetBytes(message + "\n"));
    }
}