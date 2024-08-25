using System.Threading.Channels;
using robot_project_v3.Server.BackgroundService.Command.Api;
using robot_project_v3.Server.BackgroundService.Command.Strategy;
using RobotAppLibrary.Api.Providers.Exceptions;
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
                await commandHandler.HandleApiCommand(command);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error on API {Command} execution", command.GetType().Name );
                command.SetException(e);
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
                logger.Error(ex, "Error on Strategy {Command} execution", command.GetType().Name);
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