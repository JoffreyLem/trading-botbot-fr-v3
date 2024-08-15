using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Connector.Exceptions;
using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Tests.Integrations.Api.Connector.Tcp.Fixture;
using Serilog;
using Xunit;

namespace RobotAppLibrary.Tests.Integrations.Api.Connector.Tcp;

[Collection("TcpServer collection")]
public class TcpClientTests
{
    private readonly TcpServerFixture _fixture;
    private readonly Mock<ILogger> _loggerMock;
    private readonly TcpClient _tcpClient;
    private readonly TcpConnectorWrapper _tcpConnector;
    private readonly TestTcpStreamingConnector _tcpStreamingConnector;

    public TcpClientTests(TcpServerFixture fixture)
    {
        _fixture = fixture;
        _loggerMock = new Mock<ILogger>();
        _loggerMock.Setup(logger => logger.ForContext(It.IsAny<Type>())).Returns(_loggerMock.Object);
        var server = new Server
        {
            Address = "localhost",
            MainPort = TcpServerFixture.ServerPort,
            StreamingPort = TcpServerFixture.StreamingServerPort
        };
        _tcpClient = new TcpClient(server.Address, server.MainPort, _loggerMock.Object);
        _tcpConnector = new TcpConnectorWrapper(server, _loggerMock.Object);
        _tcpStreamingConnector = new TestTcpStreamingConnector(server, _loggerMock.Object);
    }

    #region Tcp client wrapper base

    private class TcpClient(string serverAddress, int port, ILogger logger)
        : TcpClientBase(serverAddress, port, logger)
    {
        protected override bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    [Fact]
    public async Task ConnectAsync_ShouldConnectAndAuthenticate()
    {
        // Arrange
        var isConnected = false;

        // Act
        var act = async () => await _tcpClient.ConnectAsync();

        _tcpClient.Connected += (sender, args) => isConnected = true;

        // Assert
        await act.Should().NotThrowAsync();
        _tcpClient.IsConnected.Should().BeTrue();
        isConnected.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_ShouldThrowException_WhenNotConnected()
    {
        // Arrange
        var isDisconnected = false;

        // Act
        var act = async () => await _tcpClient.SendAsync("test message");
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Assert
        await act.Should().ThrowAsync<ApiCommunicationException>()
            .WithMessage("Error while sending the data (socket disconnected)");
    }

    [Fact]
    public async Task ReceiveAsync_ShouldReturnMessage()
    {
        // Act
        await _tcpClient.ConnectAsync();
        await _tcpClient.SendAsync("test message");
        var message = await _tcpClient.ReceiveAsync();

        // Assert
        message.Should().Contain("Ok");
    }

    [Fact]
    public async void Dispose_ShouldCloseClient()
    {
        // Arrange
        var tcpClient = new TcpClient("localhost", 1234, _loggerMock.Object);
        var isDisconnected = false;

        // Act
        await tcpClient.ConnectAsync();

        tcpClient.Disconnected += (sender, args) => isDisconnected = true;
        tcpClient.Close();

        // Assert
        tcpClient.IsConnected.Should().BeFalse();
        isDisconnected.Should().BeTrue();
    }

    #endregion


    #region Tcp connector

    private class TcpConnectorWrapper(Server server, ILogger logger)
        : TcpConnector(server, logger)
    {
        protected override bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    [Fact]
    public async Task SendAndReceiveAsync_ShouldLogRequestAndResponse()
    {
        // Arrange
        var messageToSend = "{\"ApiKey\":\"12345\",\"password\":\"secret\"}";
        var expectedResponse = "Ok";

        // Act
        await _tcpConnector.ConnectAsync();
        var response = await _tcpConnector.SendAndReceiveAsync(messageToSend);

        // Assert
        response.Should().Be(expectedResponse);

        _loggerMock.Verify(logger => logger.Information("Tcp log received : {@Tcp}", It.Is<TcpLog>(log =>
            log.RequestMessage == "{\"ApiKey\":\"****\",\"password\":\"****\"}" &&
            log.ResponseMessage == "Ok"
        )), Times.Once);
    }

    #endregion


    #region Tcp streaming connector test

    private class TestTcpStreamingConnector(Server server, ILogger logger) : TcpStreamingConnector(server, logger)
    {
        public bool IsReadingMessages { get; private set; }
        public bool TickReceived { get; private set; }

        protected override void HandleMessage(string message)
        {
            // Simuler le traitement des messages
            if (message == "{TickMessage}")
            {
                TickReceived = true;
                OnTickRecordReceived(new Tick());
            }
        }


        protected override void ReadStreamMessage()
        {
            IsReadingMessages = true;
            base.ReadStreamMessage();
        }

        protected override bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    [Fact]
    public async Task ConnectAsync_ShouldStartReadingMessages()
    {
        // Act
        await _tcpStreamingConnector.ConnectAsync();

        // Assert
        await Task.Delay(TimeSpan.FromMilliseconds(300));
        _tcpStreamingConnector.IsReadingMessages.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_ShouldLogMessage()
    {
        // Arrange
        var message = "Test message";

        // Act
        await _tcpStreamingConnector.ConnectAsync();
        await _tcpStreamingConnector.SendAsync(message);

        // Assert
        _loggerMock.Verify(logger => logger.Information("Streaming message to send {Message}", message), Times.Once);
    }

    [Fact]
    public async Task ReceiveMessage_ShouldInvokeHandleMessage()
    {
        // Arrange
        var tickMessage = "{TickMessage}";

        // Act
        await _tcpStreamingConnector.ConnectAsync();
        await _fixture.StreamingServer.SendMessageAsync(tickMessage);
        // Assert

        await Task.Delay(TimeSpan.FromSeconds(1));
        _tcpStreamingConnector.TickReceived.Should().BeTrue();
        _loggerMock.Verify(logger => logger.Verbose("New stream message received {@message}", tickMessage), Times.Once);
    }

    #endregion
}