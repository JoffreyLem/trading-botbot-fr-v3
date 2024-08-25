using System.Threading.Channels;
using robot_project_v3.Server.Command.Api;
using robot_project_v3.Server.Command.Strategy;
using ILogger = Serilog.ILogger;

namespace robot_project_v3.Server.BackgroundService;

public class BotBackgroundService(
    ChannelReader<CommandeBaseApiAbstract> channelApiReader,
    ChannelReader<CommandeBaseStrategyAbstract> channelStrategyReader,
    ILogger logger,
    ICommandHandler commandHandler)
    : Microsoft.Extensions.Hosting.BackgroundService
{
    private async Task ProcessApiChannel(CancellationToken stoppingToken)
    {
        await foreach (var command in channelApiReader.ReadAllAsync(stoppingToken))
            try
            {
                logger.Information("Strategy command received {Command}", command.GetType().Name);
                await commandHandler.HandleApiCommand(command);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on {Command} execution", command);
                command.SetException(ex);
            }

    }

    private async Task ProcessStrategyChannel(CancellationToken stoppingToken)
    {
        await foreach (var command in channelStrategyReader.ReadAllAsync(stoppingToken))
            try
            {
                await commandHandler.HandleStrategyCommand(command);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on {Command} execution", command);
                command.SetException(ex);
            }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiTask = ProcessApiChannel(stoppingToken);
        var strategyTask = ProcessStrategyChannel(stoppingToken);

        await Task.WhenAll(apiTask, strategyTask);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await commandHandler.Shutdown();
    }
}