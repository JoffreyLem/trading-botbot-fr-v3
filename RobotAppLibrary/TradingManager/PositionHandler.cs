using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager.Interfaces;
using Serilog;

namespace RobotAppLibrary.TradingManager;

public class PositionHandler : IPositionHandler
{
    private readonly IApiProviderBase _apiHandler;
    private readonly ILogger _logger;
    private readonly ILotValueCalculator _lotValueCalculator;
    private readonly string _symbol;
    private AccountBalance _accountBalance = new();
    private SymbolInfo _symbolInfo = null!;

    public PositionHandler(ILogger logger, IApiProviderBase apiHandler, string symbol, string strategyId,
        ILotValueCalculator lotValueCalculator)
    {
        _logger = logger.ForContext<PositionHandler>();
        _apiHandler = apiHandler;
        _symbol = symbol;
        StrategyId = strategyId;
        _lotValueCalculator = lotValueCalculator;
        Init();
    }

    private string StrategyId { get; }
    public double Risque { get; set; } = 2;
    public Position? PositionPending { get; private set; }
    public Tick LastPrice { get; private set; } = new();
    public double MaxLot { get; private set; }
    public int DefaultSl { get; set; } = 100;
    public int DefaultTp { get; set; } = 100;
    public Position PositionOpened { get; private set; }
    public bool PositionInProgress => PositionOpened is not null || PositionPending is not null;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;

    public async Task OpenPositionAsync(string symbol, TypeOperation typePosition, double volume = 0D,
        decimal sl = 0M,
        decimal tp = 0M, double? risk = null, long? expiration = 0L)
    {
        try
        {
            var priceData = typePosition == TypeOperation.Buy
                ? LastPrice.Ask.GetValueOrDefault()
                : LastPrice.Bid.GetValueOrDefault();
            sl = sl != 0M
                ? Math.Round(sl, _symbolInfo.Precision)
                : CalculateStopLoss(DefaultSl, typePosition);
            tp = tp != 0M
                ? Math.Round(tp, _symbolInfo.Precision)
                : CalculateTakeProfit(DefaultTp, typePosition);
            volume = volume != 0 ? Math.Round(volume, 2) : CalculatePositionSize(priceData, sl, risk);

            var positionModele = new Position
            {
                Symbol = symbol,
                Id = Guid.NewGuid().ToString(),
                Spread = LastPrice.Spread,
                OpenPrice = priceData, // LastPrice.Bid.GetValueOrDefault() ?????
                StopLoss = sl,
                TakeProfit = tp,
                Volume = volume,
                StrategyId = StrategyId,
                TypePosition = typePosition
            };
            PositionPending = positionModele;
            _logger.Information("Position to open {@Position}", positionModele);
            await _apiHandler.OpenPositionAsync(positionModele);
        }
        catch (Exception e)
        {
            PositionPending = null;
            _logger.Error(e, "Error on open position");
        }
    }


    public async Task UpdatePositionAsync(Position position)
    {
        try
        {
            if (position.StatusPosition is not StatusPosition.Close)
                if (PositionOpened?.StopLoss != position.StopLoss ||
                    PositionOpened?.TakeProfit != position.TakeProfit)
                {
                    position.StopLoss = Math.Round(position.StopLoss, _symbolInfo.Precision);
                    position.TakeProfit = Math.Round(position.TakeProfit, _symbolInfo.Precision);
                    position.CurrentPrice = position.TypePosition == TypeOperation.Buy ? LastPrice.Ask : LastPrice.Bid;
                    _logger.Information("Position to update {@Position}", position);
                    await _apiHandler.UpdatePositionAsync(position);
                }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Position {Id} can't be update", position.Id);
        }
    }

    public async Task ClosePositionAsync(Position position)
    {
        try
        {
            if (position.StatusPosition is not StatusPosition.Close)
            {
                var closeprice = position.TypePosition == TypeOperation.Buy ? LastPrice.Ask : LastPrice.Bid;
                position.ClosePrice = closeprice;
                _logger.Information("Position to close {@Position}", position);
                await _apiHandler.ClosePositionAsync(position);
            }
        }
        catch (Exception e)
        {
            position.StatusPosition = StatusPosition.Open;
            _logger.Error(e, "Position {PositionId} close error api", position.Id);
        }
    }


    public decimal CalculateStopLoss(decimal pips, TypeOperation positionType)
    {
        if (_symbolInfo is { Precision: > 1, Category: Category.Forex }) pips *= (decimal)_symbolInfo.TickSize;

        return positionType switch
        {
            TypeOperation.Buy => Math.Round(LastPrice.Bid.GetValueOrDefault() - pips, _symbolInfo.Precision),
            TypeOperation.Sell => Math.Round(LastPrice.Ask.GetValueOrDefault() + pips, _symbolInfo.Precision),
            _ => throw new ArgumentException("Invalid position type")
        };
    }


    public decimal CalculateTakeProfit(decimal pips, TypeOperation positionType)
    {
        if (_symbolInfo is { Precision: > 1, Category: Category.Forex }) pips *= (decimal)_symbolInfo.TickSize;

        return positionType switch
        {
            TypeOperation.Buy => Math.Round(LastPrice.Ask.GetValueOrDefault() + pips, _symbolInfo.Precision),
            TypeOperation.Sell => Math.Round(LastPrice.Bid.GetValueOrDefault() - pips, _symbolInfo.Precision),
            _ => throw new ArgumentException("Invalid position type")
        };
    }

    public void Dispose()
    {
        _apiHandler.NewBalanceEvent -= ApiHandlerOnNewBalanceEvent;
        _lotValueCalculator.Dispose();
    }

    private double CalculatePositionSize(decimal entryPrice, decimal stopLossPrice, double? risk = null)
    {
        var riskValue = risk ?? Risque;

        var riskMoney = riskValue / 100 * _accountBalance.Equity.GetValueOrDefault();
        double positionSize;

        if (_symbolInfo.Category == Category.Forex)
            positionSize = CalculateForexPositionSize(entryPrice, stopLossPrice, riskMoney);
        else
            positionSize = CalculateOtherPositionSize(entryPrice, stopLossPrice, riskMoney);

        if (positionSize < _symbolInfo.LotMin) return _symbolInfo.LotMin.GetValueOrDefault();

        return Math.Round(Math.Min(positionSize, MaxLot), 2);
    }

    private double CalculateForexPositionSize(decimal entryPrice, decimal stopLossPrice, double riskMoney)
    {
        var pipsRisk = Math.Abs(entryPrice - stopLossPrice) / Convert.ToDecimal(_symbolInfo.TickSize);
        var riskValue = pipsRisk * (decimal)_lotValueCalculator.PipValueStandard;
        var positionSizeByRisk = riskMoney / (double)riskValue;
        var maxPositionSizeByMargin = _accountBalance.Equity.GetValueOrDefault() / _lotValueCalculator.MarginPerLot;

        return Math.Min(positionSizeByRisk, maxPositionSizeByMargin) - 0.01;
    }

    private double CalculateOtherPositionSize(decimal entryPrice, decimal stopLossPrice, double riskMoney)
    {
        var stopLossPoints = Math.Abs(entryPrice - stopLossPrice);
        var lossPerStopLoss = _lotValueCalculator.PipValueStandard * (double)stopLossPoints;
        var positionSizeByRisk = riskMoney / lossPerStopLoss;
        var maxPositionSizeByMargin = _accountBalance.Equity.GetValueOrDefault() / _lotValueCalculator.MarginPerLot;

        return Math.Min(positionSizeByRisk, maxPositionSizeByMargin);
    }

    private void Init()
    {
        _symbolInfo = _apiHandler.GetSymbolInformationAsync(_symbol).Result;
        LastPrice = _apiHandler.GetTickPriceAsync(_symbol).Result;
        _apiHandler.TickEvent += ApiHandlerOnTickEvent;
        _apiHandler.PositionOpenedEvent += ApiHandlerOnPositionOpenedEvent;
        _apiHandler.PositionUpdatedEvent += ApiHandlerOnPositionUpdatedEvent;
        _apiHandler.PositionRejectedEvent += ApiHandlerOnPositionRejectedEvent;
        _apiHandler.PositionClosedEvent += ApiHandlerOnPositionClosedEvent;
        _apiHandler.NewBalanceEvent += ApiHandlerOnNewBalanceEvent;

        var currentPosition = _apiHandler.GetOpenedTradesAsync(StrategyId).Result;

        if (currentPosition is not null)
        {
            _apiHandler.RestoreSession(currentPosition);
            PositionOpened = currentPosition;
        }

        _accountBalance = _apiHandler.GetBalanceAsync().GetAwaiter().GetResult();
        UpdateMaxLot();
    }


    private void ApiHandlerOnTickEvent(object? sender, Tick e)
    {
        if (e.Symbol == _symbol) LastPrice = e;
    }

    private void ApiHandlerOnPositionOpenedEvent(object? sender, Position e)
    {
        if (e.PositionStrategyReferenceId == PositionPending?.PositionStrategyReferenceId)
        {
            PositionOpened = e;
            e.StatusPosition = StatusPosition.Open;
            PositionPending = null;
            _logger.Information("Position opened : {@Position}", e);
            PositionOpenedEvent?.Invoke(this, e);
        }
    }

    private void ApiHandlerOnNewBalanceEvent(object? sender, AccountBalance e)
    {
        _accountBalance = e;
        UpdateMaxLot();
    }

    private void UpdateMaxLot()
    {
        MaxLot = Math.Round(_accountBalance.Balance / _lotValueCalculator.MarginPerLot, 2);
    }

    private void ApiHandlerOnPositionRejectedEvent(object? sender, Position e)
    {
        if (e.PositionStrategyReferenceId == PositionPending?.PositionStrategyReferenceId)
        {
            PositionPending = null;
            e.StatusPosition = StatusPosition.Rejected;
            PositionRejectedEvent?.Invoke(this, e);
            _logger.Information("Position rejected : {Position}", e);
        }
    }


    private void ApiHandlerOnPositionUpdatedEvent(object? sender, Position e)
    {
        if (PositionOpened is not null && e.PositionStrategyReferenceId == PositionOpened?.PositionStrategyReferenceId)
        {
            bool shouldLog = PositionOpened.StopLoss != e.StopLoss || PositionOpened.TakeProfit != e.TakeProfit;
            PositionOpened.Profit = e.Profit;
            PositionOpened.StopLoss = e.StopLoss;
            PositionOpened.TakeProfit = e.TakeProfit;
            PositionUpdatedEvent?.Invoke(this, e);
            if (shouldLog)
            {
               _logger.Information("Position updated {@Position}", e);
            }
            else
            {
                _logger.Debug("Position updated : {@Position}", e);
            }
        }
    }


    private void ApiHandlerOnPositionClosedEvent(object? sender, Position e)
    {
        if (PositionOpened is not null && PositionOpened?.PositionStrategyReferenceId == e.PositionStrategyReferenceId)
        {
            PositionOpened = null;
            PositionClosedEvent?.Invoke(this, e);
            _logger.Information("Position Closed : {@Position}", e);
        }
    }
}