using System.Threading.Channels;
using Moq;
using robot_project_v3.Server.BackgroundService;
using robot_project_v3.Server.Command.Api;
using robot_project_v3.Server.Command.Strategy;
using Serilog;

namespace robot_project_v3.Server.Tests.BackgroundService;

public class BotBackgroundServiceTests
{
    private readonly Channel<CommandeBaseApiAbstract> _channelApi;
    private readonly Channel<CommandeBaseStrategyAbstract> _channelStrategy;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<ICommandHandler> _mockCommandHandler;
    private readonly BotBackgroundService _botBackgroundService;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public BotBackgroundServiceTests()
    {
        _channelApi = Channel.CreateUnbounded<CommandeBaseApiAbstract>();
        _channelStrategy = Channel.CreateUnbounded<CommandeBaseStrategyAbstract>();
        _mockLogger = new Mock<ILogger>();
        _mockCommandHandler = new Mock<ICommandHandler>();
        _cancellationTokenSource = new CancellationTokenSource();

        _botBackgroundService = new BotBackgroundService(
            _channelApi.Reader,
            _channelStrategy.Reader,
            _mockLogger.Object,
            _mockCommandHandler.Object);
    }

    [Fact]
    public async Task ProcessApiChannel_ShouldLogAndHandleCommand()
    {
        // Arrange
        var command = new Mock<CommandeBaseApiAbstract>();
        await _channelApi.Writer.WriteAsync(command.Object);

        // Act
        var serviceTask = _botBackgroundService.StartAsync(_cancellationTokenSource.Token);

        // Allow some time for the command to be processed
        await Task.Delay(100);

        // Stop the service
        await _cancellationTokenSource.CancelAsync();
        await serviceTask;

        // Assert
        _mockLogger.Verify(x => x.Information("Strategy command received {Command}", command.Object), Times.Once);
        _mockCommandHandler.Verify(x => x.HandleApiCommand(command.Object), Times.Once);
    }

    [Fact]
    public async Task ProcessApiChannel_ShouldLogErrorOnException()
    {
        // Arrange
        var command = new Mock<CommandeBaseApiAbstract>();
        var exception = new Exception("Test exception");
        await _channelApi.Writer.WriteAsync(command.Object);

        _mockCommandHandler
            .Setup(x => x.HandleApiCommand(command.Object))
            .ThrowsAsync(exception);

        // Act
        var serviceTask = _botBackgroundService.StartAsync(_cancellationTokenSource.Token);

        // Allow some time for the command to be processed
        await Task.Delay(100);

        // Stop the service
        await _cancellationTokenSource.CancelAsync();
        await serviceTask;

        // Assert
        _mockLogger.Verify(x => x.Error(exception, "Error on {Command} execution", command.Object), Times.Once);
        command.Verify(x => x.SetException(exception), Times.Once);
    }

    [Fact]
    public async Task ProcessStrategyChannel_ShouldLogAndHandleCommand()
    {
        // Arrange
        var command = new Mock<CommandeBaseStrategyAbstract>();
        await _channelStrategy.Writer.WriteAsync(command.Object);

        // Act
        var serviceTask = _botBackgroundService.StartAsync(_cancellationTokenSource.Token);

        // Allow some time for the command to be processed
        await Task.Delay(100);

        // Stop the service
        await _cancellationTokenSource.CancelAsync();
        await serviceTask;

        // Assert
        _mockLogger.Verify(x => x.Information("Api command received {Command}", command.Object), Times.Once);
        _mockCommandHandler.Verify(x => x.HandleStrategyCommand(command.Object), Times.Once);
    }

    [Fact]
    public async Task ProcessStrategyChannel_ShouldLogErrorOnException()
    {
        // Arrange
        var command = new Mock<CommandeBaseStrategyAbstract>();
        var exception = new Exception("Test exception");
        await _channelStrategy.Writer.WriteAsync(command.Object);

        _mockCommandHandler
            .Setup(x => x.HandleStrategyCommand(command.Object))
            .ThrowsAsync(exception);

        // Act
        var serviceTask = _botBackgroundService.StartAsync(_cancellationTokenSource.Token);

        // Allow some time for the command to be processed
        await Task.Delay(100);

        // Stop the service
        await _cancellationTokenSource.CancelAsync();
        await serviceTask;

        // Assert
        _mockLogger.Verify(x => x.Error(exception, "Error on {Command} execution", command.Object), Times.Once);
        command.Verify(x => x.SetException(exception), Times.Once);
    }
}