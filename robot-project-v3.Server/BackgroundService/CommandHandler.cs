using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using robot_project_v3.Database.Modeles;
using robot_project_v3.Mail;
using robot_project_v3.Server.Command;
using robot_project_v3.Server.Command.Api;
using robot_project_v3.Server.Command.Strategy;
using robot_project_v3.Server.Dto.Enum;
using robot_project_v3.Server.Dto.Response;
using robot_project_v3.Server.Hubs;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Factory;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Strategy;
using RobotAppLibrary.StrategyDynamicCompiler;
using ILogger = Serilog.ILogger;

namespace robot_project_v3.Server.BackgroundService;

public interface ICommandHandler
{
    Task HandleApiCommand(CommandeBaseApiAbstract command);

    Task HandleStrategyCommand(CommandeBaseStrategyAbstract command);

    Task Shutdown();
}

public class CommandHandler(IHubContext<HubInfoClient, IHubInfoClient> hubContext, ILogger logger, IMapper mapper, IEmailService _emailService)
    : ICommandHandler
{
    private readonly ILogger _logger = logger.ForContext<CommandHandler>();

    private readonly Dictionary<string, IStrategyBase> _strategyList = new();
    private readonly Dictionary<string, CustomLoadContext> _strategyListContext = new();
    
    private  IApiProviderBase? _apiProviderBase;
    private readonly IMapper _mapper = mapper;
    #region API

    public async Task HandleApiCommand(CommandeBaseApiAbstract command)
    {
        switch (command)
        {
            case DisconnectCommand disconnectCommand:
                await Disconnect(disconnectCommand);
                break;
            case GetAllSymbolCommand getAllSymbolCommand:
                await GetAllSymbol(getAllSymbolCommand);
                break;
            case GetTypeProviderCommand getTypeHandlerCommand:
                GetTypeHandler(getTypeHandlerCommand);
                break;
            case IsConnectedCommand isConnectedCommand:
                IsConnected(isConnectedCommand);
                break;
            case ApiConnectCommand apiConnectCommand:
                await Connect(apiConnectCommand);
                break;
            default:
                _logger.Error("Trying to use unhandled command {@Command}", command);
                throw new CommandException("Internal error");
        }
    }
    
    private void GetTypeHandler(GetTypeProviderCommand getTypeHandlerCommand)
    {
        CheckApiHandlerNotNull();
        var data = _apiProviderBase!.ApiProviderName.ToString();
        _logger.Information("Api handler type is {Data}", data);
        getTypeHandlerCommand.ResponseSource.SetResult(data);
    }
    
    private async Task GetAllSymbol(GetAllSymbolCommand command)
    {
        CheckApiHandlerNotNull();
        var symbols = await _apiProviderBase!.GetAllSymbolsAsync();

        command.ResponseSource.SetResult(symbols);
    }
    
    private void IsConnected(IsConnectedCommand isConnectedCommand)
    {
        if (_apiProviderBase is null || !(bool)_apiProviderBase?.IsConnected())
            isConnectedCommand.ResponseSource.SetResult(false);
        else
            isConnectedCommand.ResponseSource.SetResult(true);
    }
    
    private async Task Disconnect(DisconnectCommand command)
    {
        CheckApiHandlerNotNull();
        await _apiProviderBase!.DisconnectAsync();
        //_apiProviderBase.Connected -= ApiHandlerBaseOnConnected;
        _apiProviderBase.Disconnected -= ApiHandlerBaseOnDisconnected;
        _apiProviderBase.NewBalanceEvent -= ApiProviderBaseOnNewBalanceEvent ;
        _apiProviderBase.Dispose();
        _apiProviderBase = null;

        command.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private async void ApiProviderBaseOnNewBalanceEvent(object? sender, AccountBalance e)
    {
        var balanceDto = new AccountBalanceDto()
        {
            Balance = e.Balance,
            Credit = e.Credit,
            Equity = e.Equity,
            Margin = e.Margin,
            MarginFree = e.MarginFree,
            MarginLevel = e.MarginLevel
        };

        await hubContext.Clients.All.ReceiveBalance(balanceDto);
    }

    private async Task Connect(ApiConnectCommand command)
    {
        _logger.Information("Init handler to type {Enum}", command.ConnectDto.HandlerEnum);
        _apiProviderBase = ApiProviderFactory.GetApiHandler(command.ConnectDto.HandlerEnum.GetValueOrDefault(), _logger);
        var credentials = new Credentials
        {
            User = command.ConnectDto.User,
            Password = command.ConnectDto.Pwd
        };
        await _apiProviderBase.ConnectAsync(credentials);
        _apiProviderBase.Connected += (_, _) => {};
        _apiProviderBase.Disconnected += ApiHandlerBaseOnDisconnected;
        _apiProviderBase.NewBalanceEvent += ApiProviderBaseOnNewBalanceEvent;

        command.ResponseSource.SetResult(new AcknowledgementResponse());
    }
    
    private async void ApiHandlerBaseOnDisconnected(object? sender, EventArgs e)
    {
        try
        {
            foreach (var keyPairValue in _strategyList)
            {
                var strategy = keyPairValue.Value;
                await strategy.DisableStrategy(StrategyReasonDisabled.Api);
            }

            _strategyList.Clear();
            foreach (var customLoadContext in _strategyListContext) customLoadContext.Value.Unload();
            _strategyListContext.Clear();
            _apiProviderBase?.Dispose();
            _apiProviderBase = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Can't close strategy after api disconnection");
        }
    }
    
    private void CheckApiHandlerNotNull()
    {
        if (_apiProviderBase is null) throw new CommandException("The Api handler is not connected");
    }

    #endregion

    #region Strategy

    public async Task HandleStrategyCommand(CommandeBaseStrategyAbstract command)
    {
        switch (command)
        {
            case InitStrategyCommand initStrategyCommandDto:
                CheckApiHandlerNotNull();
                InitStrategy(initStrategyCommandDto);
                _logger.Information("Strategy command processed {@Command}", initStrategyCommandDto);
                break;
            case GetStrategyInfoCommand getStrategyInfoCommand:
                GetStrategyInfo(getStrategyInfoCommand, GetStrategyById(getStrategyInfoCommand.Id));
                _logger.Information("Strategy command processed {@Command}", getStrategyInfoCommand);
                break;
            case CloseStrategyCommand closeStrategyCommand:
                await CloseStrategy(closeStrategyCommand, GetStrategyById(closeStrategyCommand.Id));
                _logger.Information("Strategy command processed {@Command}", closeStrategyCommand);
                break;
            case GetStrategyResultCommand getStrategyResultRequestCommand:
                GetStrategyResult(getStrategyResultRequestCommand, GetStrategyById(getStrategyResultRequestCommand.Id));
                _logger.Information("Strategy command processed {@Command}", getStrategyResultRequestCommand);
                break;
            case GetOpenedPositionCommand getOpenedPositionRequestCommand:
                GetOpenedPosition(getOpenedPositionRequestCommand, GetStrategyById(getOpenedPositionRequestCommand.Id));
                _logger.Information("Strategy command processed {@Command}", getOpenedPositionRequestCommand);
                break;
            case SetCanRunCommand setCanRunCommand:
                SetCanRun(setCanRunCommand, GetStrategyById(setCanRunCommand.Id));
                _logger.Information("Strategy command processed {@Command}", setCanRunCommand);
                break;
            case GetChartCommand getChartCommandRequest:
                 GetChart(getChartCommandRequest, GetStrategyById(getChartCommandRequest.Id));
                _logger.Information("Strategy command processed {@Command}", getChartCommandRequest);
                break;
            case GetAllStrategyCommand getAllStrategyCommandRequest:
                GetAllStrategy(getAllStrategyCommandRequest);
                _logger.Information("Strategy command processed {@Command}", getAllStrategyCommandRequest);
                break;
            case RunStrategyBacktestCommand runStrategyBacktestCommand:
                RunBackTest(runStrategyBacktestCommand,GetStrategyById(runStrategyBacktestCommand.Id));
                _logger.Information("Api command processed {@Command}", runStrategyBacktestCommand);
                break;
            case GetBacktestResultCommand strategyResultBacktestCommand:
                GetStrategyBacktestResult(strategyResultBacktestCommand,GetStrategyById(strategyResultBacktestCommand.Id));
                _logger.Information("Api command processed {@Command}", strategyResultBacktestCommand);
                break;

            default:
                _logger.Error("Trying to use unanhdled command {@Command}", command);
                throw new CommandException("Internal error");
        }
    }
    
    
    private (StrategyImplementationBase, CustomLoadContext) GenerateStrategy(StrategyFile strategyFileDto)
    {
        var sourceCode = Encoding.UTF8.GetString(strategyFileDto.Data);
        var compiledCode = StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode);

        var context = new CustomLoadContext();
        using var stream = new MemoryStream(compiledCode);
        var assembly = context.LoadFromStream(stream);

        var className = StrategyDynamiqCompiler.GetFirstClassName(sourceCode);

        var type = assembly.GetType(className);
        var instance = Activator.CreateInstance(type);

        return ((StrategyImplementationBase)instance, context);
    }
    
    private async void StrategyBaseOnPositionRejectedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Rejected;
        await hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnPositionClosedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Closed;
        await hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnPositionUpdatedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Updated;
        await hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnPositionOpenedEvent(object? sender, RobotEvent<Position> e)
    {
        var posDto = _mapper.Map<PositionDto>(e.EventField);
        posDto.PositionState = PositionStateEnum.Opened;
        await hubContext.Clients.All.ReceivePosition(posDto);
    }

    private async void StrategyBaseOnCandleEvent(object? sender, RobotEvent<Candle> e)
    {
        var candleDto = _mapper.Map<CandleDto>(e.EventField);
        await hubContext.Clients.All.ReceiveCandle(candleDto);
    }

    private async void StrategyBaseOnTickEvent(object? sender, RobotEvent<Tick> e)
    {
        var tickDto = _mapper.Map<TickDto>(e.EventField);
        await hubContext.Clients.All.ReceiveTick(tickDto);
    }
    
    private void StrategyBaseOnStrategyDisabled(object? sender, RobotEvent<string> e)
    {
        _logger.Warning("{Message}, send email to user", e.EventField);
        _emailService.SendEmail("Strategy disabled", e.EventField).GetAwaiter().GetResult();
   
        //StrategyDisabled?.Invoke(this, new RobotEvent<string>(e.EventField, e.Id));
    }

    
    private void InitStrategy(InitStrategyCommand initStrategyCommandDto)
    {
        try
        {
            var strategyImplementation = GenerateStrategy(initStrategyCommandDto.StrategyFileDto);
            var istrategySerrvice = new StrategyServiceFactory();
            var strategyBase = new StrategyBase(initStrategyCommandDto.Symbol, strategyImplementation.Item1, _apiProviderBase,
                logger,istrategySerrvice);

            strategyBase.TickEvent += StrategyBaseOnTickEvent;
            strategyBase.CandleEvent += StrategyBaseOnCandleEvent;
            strategyBase.PositionOpenedEvent += StrategyBaseOnPositionOpenedEvent;
            strategyBase.PositionUpdatedEvent += StrategyBaseOnPositionUpdatedEvent;
            strategyBase.PositionClosedEvent += StrategyBaseOnPositionClosedEvent;
            strategyBase.PositionRejectedEvent += StrategyBaseOnPositionRejectedEvent;
            strategyBase.StrategyDisabledEvent += StrategyBaseOnStrategyDisabled;

            _strategyList.Add(strategyBase.Id, strategyBase);
            _strategyListContext.Add(strategyBase.Id, strategyImplementation.Item2);

            initStrategyCommandDto.ResponseSource.SetResult(new AcknowledgementResponse());
        }
        catch (Exception e) when (e is not StrategyException)
        {
            throw;
        }
    }
    
    private void GetStrategyInfo(GetStrategyInfoCommand getStrategyInfoCommand, IStrategyBase strategy)
    {
        getStrategyInfoCommand.ResponseSource.SetResult(_mapper.Map<StrategyInfoDto>(strategy));
    }
    
    private IStrategyBase GetStrategyById(string id)
    {
        if (_strategyList.TryGetValue(id, out var strategyBase))
            return strategyBase;
        throw new CommandException($"The strategy {id} is not initialized");
    }
    
    private async Task CloseStrategy(CloseStrategyCommand closeStrategyCommand, IStrategyBase strategy)
    {
        await strategy.DisableStrategy(StrategyReasonDisabled.User);
        _strategyList.Remove(closeStrategyCommand.Id);
        _strategyListContext[closeStrategyCommand.Id].Unload();
        _strategyListContext.Remove(closeStrategyCommand.Id);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        closeStrategyCommand.ResponseSource.SetResult(new AcknowledgementResponse());
    }

    private void GetStrategyResult(GetStrategyResultCommand strategyResultRequest, IStrategyBase strategy)
    {
        var globalResultDto = new GlobalResultsDto()
        {
            Result = _mapper.Map<ResultDto>(strategy.Results),
            Positions = _mapper.Map<List<PositionDto>>(strategy.Results.Positions.ToList()),
            MonthlyResults = _mapper.Map<List<MonthlyResultDto>>(strategy.Results.MonthlyResults)
        };
        strategyResultRequest.ResponseSource.SetResult(globalResultDto);
    }
    
    private void GetOpenedPosition(GetOpenedPositionCommand command, IStrategyBase strategy)
    {
        var listPositionsDto = new List<PositionDto>();

        if (strategy.PositionOpened is not null)
        {
            var position = _mapper.Map<PositionDto>(strategy.PositionOpened);
            listPositionsDto.Add(position);
        }

        command.ResponseSource.SetResult(listPositionsDto);
    }
    
    private void SetCanRun(SetCanRunCommand setCanRunCommand, IStrategyBase strategy)
    {
        strategy.CanRun = setCanRunCommand.Bool;
        setCanRunCommand.ResponseSource.SetResult(new AcknowledgementResponse());
    }
    
    private void GetChart(GetChartCommand chartCommandRequest, IStrategyBase strategy)
    {
        var candles = strategy.Chart.Select(x => new CandleDto
        {
            Open = (double)x.Open,
            High = (double)x.High,
            Low = (double)x.Low,
            Close = (double)x.Close,
            Date = x.Date,
            Volume = (double)x.Volume
        }).ToList();
        chartCommandRequest.ResponseSource.SetResult(candles);
    }

    
    private void GetAllStrategy(GetAllStrategyCommand getAllStrategyCommandRequest)
    {
        var response = new List<StrategyInfoDto>();
        if (_strategyList is { Count: 0 })
        {
            getAllStrategyCommandRequest.ResponseSource.SetResult(response);
        }
        else
        {
            foreach (var (key, value) in _strategyList)
            {
                var strategy = GetStrategyById(key);
                var strategyInfoDto = _mapper.Map<StrategyInfoDto>(strategy);
                response.Add(strategyInfoDto);
            }
            getAllStrategyCommandRequest.ResponseSource.SetResult(response);
        }
    }
    
    private void RunBackTest(RunStrategyBacktestCommand runStrategyBacktestCommand, IStrategyBase strategy)
    {
        // TODO : a implémenter plus tard
    }
    
    private void GetStrategyBacktestResult(GetBacktestResultCommand strategyResultBacktestCommand,
        IStrategyBase strategy)
    {
        // TODO : Reimplémenter
    }


    #endregion

  

    public async Task Shutdown()
    {
        if (_apiProviderBase is not null) await _apiProviderBase.DisconnectAsync();
    }
}