using System.Threading.Channels;
using robot_project_v3.Database.Repositories;
using robot_project_v3.Server.Command.Strategy;
using robot_project_v3.Server.Dto;
using ILogger = Serilog.ILogger;

namespace robot_project_v3.Server.Services;

public interface IStrategyService
{
    Task InitStrategy(StrategyInitDto strategyInitDto);
    Task<List<StrategyInfoDto>> GetAllStrategy();
    Task CloseStrategy(string id);
    Task<StrategyInfoDto> GetStrategyInfo(string id);
    Task<GlobalResultsDto> GetResult(string id);
    Task SetCanRun(string id, bool value);
    Task<List<PositionDto>> GetOpenedPositions(string id);
    Task<BackTestDto> RunBackTest(string id, BackTestRequestDto backTestRequestDto);
    Task<BackTestDto> GetBacktestResult(string id);
}

public class StrategyService(
    ChannelWriter<CommandeBaseStrategyAbstract> channelStrategyWriter,
    ILogger logger,
    IStrategyFileRepository strategyFileRepository)
    : IStrategyService
{
    private readonly ILogger _logger = logger.ForContext<StrategyService>();

    public async Task InitStrategy(StrategyInitDto strategyInitDto)
    {
        var strategyFile = await strategyFileRepository.GetByIdAsync(int.Parse(strategyInitDto.StrategyFileId));
        var initStrategyCommand = new InitStrategyCommand
        {
            Data = new InitStrategyDto()
            {
                StrategyFileDto = strategyFile,
                Symbol = strategyInitDto.Symbol
            },
        };

        await channelStrategyWriter.WriteAsync(initStrategyCommand);
        await initStrategyCommand.ResponseSource.Task;
    }

    public async Task<List<StrategyInfoDto>> GetAllStrategy()
    {
        var allStrategyCommand = new GetAllStrategyCommand();

        await channelStrategyWriter.WriteAsync(allStrategyCommand);

        return await allStrategyCommand.ResponseSource.Task;
    }

    public async Task CloseStrategy(string id)
    {
        var closeStrategyCommand = new CloseStrategyCommand
        {
            Id = id
        };

        await channelStrategyWriter.WriteAsync(closeStrategyCommand);

        await closeStrategyCommand.ResponseSource.Task;
    }

    public async Task<StrategyInfoDto> GetStrategyInfo(string id)
    {
        var getStrategyInfoCommand = new GetStrategyInfoCommand
        {
            Id = id
        };

        await channelStrategyWriter.WriteAsync(getStrategyInfoCommand);

        var result = await getStrategyInfoCommand.ResponseSource.Task;

        return result;
    }

    public async Task<GlobalResultsDto> GetResult(string id)
    {
        var resultCommand = new GetStrategyResultCommand
        {
            Id = id
        };

        await channelStrategyWriter.WriteAsync(resultCommand);

        var result = await resultCommand.ResponseSource.Task;

        return new GlobalResultsDto
        {
            Positions = result.Positions,
            Result = result.Result,
            MonthlyResults = result.MonthlyResults
        };
    }

    public async Task SetCanRun(string id, bool value)
    {
        var setCanRunCommand = new SetCanRunCommand
        {
            Data = value,
            Id = id
        };


        await channelStrategyWriter.WriteAsync(setCanRunCommand);

        await setCanRunCommand.ResponseSource.Task;
    }

    public async Task<List<PositionDto>> GetOpenedPositions(string id)
    {
        var command = new GetOpenedPositionCommand
        {
            Id = id
        };

        await channelStrategyWriter.WriteAsync(command);
        return await command.ResponseSource.Task;
    }

    public Task<BackTestDto> RunBackTest(string id, BackTestRequestDto backTestRequestDto)
    {
        // TODO : A faire
        throw new NotImplementedException();
    }

    public Task<BackTestDto> GetBacktestResult(string id)
    {
        // TODO : A faire
        throw new NotImplementedException();
    }
}