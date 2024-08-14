using System.Threading.Channels;
using robot_project_v3.Server.Command.Api;
using robot_project_v3.Server.Dto.Response;
using RobotAppLibrary.Api.Providers;
using RobotAppLibrary.Modeles;

namespace robot_project_v3.Server.Services;

public interface IApiProviderService
{
    Task Connect(ConnectDto connectDto);

    Task Disconnect();

    Task<bool> IsConnected();

    Task<string?> GetTypeProvider();

    Task<List<string>> GetListProvider();

    Task<List<SymbolInfo>> GetAllSymbol();
}

public class ApiProviderService(ChannelWriter<CommandeBaseApiAbstract> channelApiWriter, Serilog.ILogger logger)
    : IApiProviderService
{
    private readonly Serilog.ILogger _logger = logger.ForContext<ApiProviderService>();

    public async Task Connect(ConnectDto connectDto)
    {
        var connecCommand = new ApiConnectCommand
        {
            ConnectDto = connectDto
        };
        
        await channelApiWriter.WriteAsync(connecCommand);
        await connecCommand.ResponseSource.Task;
    }

    public async Task Disconnect()
    {
        var disconenctCommand = new DisconnectCommand();

        await channelApiWriter.WriteAsync(disconenctCommand);
        await disconenctCommand.ResponseSource.Task;
    }

    public async Task<bool> IsConnected()
    {
        var isConnectedCommand = new IsConnectedCommand();

        await channelApiWriter.WriteAsync(isConnectedCommand);
        var result = await isConnectedCommand.ResponseSource.Task;
        return result;
    }

    public async Task<string?> GetTypeProvider()
    {
        var getTypeProviderCommand = new GetTypeProviderCommand();

        await channelApiWriter.WriteAsync(getTypeProviderCommand);

        var result = await getTypeProviderCommand.ResponseSource.Task;
        return result;
    }

    public Task<List<string>> GetListProvider()
    {
        return Task.FromResult(Enum.GetNames(typeof(ApiProviderEnum)).ToList());
    }

    public async Task<List<SymbolInfo>> GetAllSymbol()
    {
        var getAllSymbolCommand = new GetAllSymbolCommand();

        await channelApiWriter.WriteAsync(getAllSymbolCommand);

        var result = await getAllSymbolCommand.ResponseSource.Task;
        return result;
    }
}