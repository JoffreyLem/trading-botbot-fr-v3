using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager.Exceptions;
using Serilog;

namespace RobotAppLibrary.TradingManager;

public interface ILotValueCalculator : IDisposable
{
    public double PipValueStandard { get; }
    public double PipValueMiniLot => PipValueStandard / 10;
    public double PipValueMicroLot => PipValueStandard / 100;
    public double PipValueNanoLot => PipValueStandard / 1000;
    double MarginPerLot { get; }
    Tick? TickPriceSecondary { get; }
}

public class LotValueCalculator : ILotValueCalculator
{
    private const int StandardLotSize = 100000;

    private readonly IApiProviderBase _apiHandler;
    private readonly ILogger? _logger;
    private bool _disposed;
    private string? _secondarySymbolAccount;
    private Tick _tickPriceMain = new();

    public LotValueCalculator(IApiProviderBase apiHandler, ILogger? logger, string symbol)
    {
        _apiHandler = apiHandler;
        _logger = logger;
        InitAsync(symbol).Wait();
    }

    private string BaseSymbolAccount { get; } = "EUR";
    private SymbolInfo SymbolInfo { get; set; } = null!;
    public double PipValueStandard { get; private set; }
    public double PipValueMiniLot => PipValueStandard / 10;
    public double PipValueMicroLot => PipValueStandard / 100;
    public double PipValueNanoLot => PipValueStandard / 1000;
    public double MarginPerLot { get; private set; }
    public Tick? TickPriceSecondary { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async Task InitAsync(string symbol)
    {
        SymbolInfo = await _apiHandler.GetSymbolInformationAsync(symbol);
        _tickPriceMain =
            await _apiHandler.GetTickPriceAsync(SymbolInfo.Symbol ??
                                                throw new InvalidOperationException("Symbol is not defined"));

        if ((SymbolInfo.Category == Category.Forex && !SymbolInfo.Symbol.Contains(BaseSymbolAccount)) ||
            (SymbolInfo.Category != Category.Forex && SymbolInfo.CurrencyProfit != BaseSymbolAccount))
            await SubscribeSecondaryPriceAsync();

        SymbolSwitch();
        _apiHandler.TickEvent += ApiHandlerOnTickEvent;
    }

    private async Task SubscribeSecondaryPriceAsync()
    {
        var symbol1 = BaseSymbolAccount;
        var symbol2 = SymbolInfo.CurrencyProfit;
        if (symbol2 != null) _secondarySymbolAccount = GetMatchingSymbolWithCurrency(symbol1, symbol2);
        if (_secondarySymbolAccount != null)
        {
            TickPriceSecondary = await _apiHandler.GetTickPriceAsync(_secondarySymbolAccount);
            _apiHandler.SubscribePrice(_secondarySymbolAccount);
        }
    }

    private void SymbolSwitch()
    {
        switch (SymbolInfo.Category)
        {
            case Category.Forex:
                HandleForex();
                break;
            default:
                HandleOtherSymbols();
                break;
        }
    }

    private void ApiHandlerOnTickEvent(object? sender, Tick e)
    {
        try
        {
            if (e.Symbol == SymbolInfo.Symbol)
            {
                _tickPriceMain = e;
                SymbolSwitch();
            }

            if (e.Symbol == _secondarySymbolAccount)
            {
                TickPriceSecondary = e;
                SymbolSwitch();
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error when updating lot value");
        }
    }

    private void HandleForex()
    {
        var tickSize = SymbolInfo.Symbol.Contains("JPY") ? 0.01m : 0.0001m;
        var pipValue = tickSize * StandardLotSize;

        if (SymbolInfo.CurrencyProfit == BaseSymbolAccount)
        {
            PipValueStandard = (double)pipValue;
        }
        else
        {
            if (SymbolInfo.Currency == BaseSymbolAccount)
                PipValueStandard = (double)(pipValue / _tickPriceMain.Bid.GetValueOrDefault());
            else if (TickPriceSecondary != null)
                PipValueStandard = (double)(pipValue / TickPriceSecondary.Bid.GetValueOrDefault());
        }

        _logger?.Debug("New lot value forex : {Lot}", PipValueStandard);

        var leverageRatio = SymbolInfo.Leverage != 0 ? 100 / SymbolInfo.Leverage : 0;
        MarginPerLot = leverageRatio > 0
            ? SymbolInfo.Leverage * StandardLotSize / 100
            : PipValueStandard * StandardLotSize;

        _logger?.Debug("Required margin per standard lot: {MarginPerLot}", MarginPerLot);
    }

    private void HandleOtherSymbols()
    {
        PipValueStandard = SymbolInfo.CurrencyProfit == BaseSymbolAccount
            ? SymbolInfo.ContractSize.GetValueOrDefault()
            : (double)(SymbolInfo.ContractSize.GetValueOrDefault() / TickPriceSecondary?.Bid.GetValueOrDefault() ?? 1);

        _logger?.Debug("New lot value indices : {Lot}", PipValueStandard);

        var leverageRatio = 100 / SymbolInfo.Leverage;
        MarginPerLot = PipValueStandard * (double)_tickPriceMain.Bid.GetValueOrDefault() / leverageRatio;

        _logger?.Debug("Required margin per lot : {MarginPerLot}", MarginPerLot);
    }

    private string GetMatchingSymbolWithCurrency(string symbol1, string symbol2)
    {
        try
        {
            var allSymbols = _apiHandler.GetAllSymbolsAsync().Result;
            var selectedSymbol =
                allSymbols.FirstOrDefault(x => x.Symbol.StartsWith(symbol1) && x.Symbol.EndsWith(symbol2));
            return selectedSymbol?.Symbol ?? throw new Exception($"No matching symbol for {symbol1} : {symbol2}");
        }
        catch (Exception e)
        {
            throw new TradingManagerException("Error fetching matching symbol", e);
        }
    }

    [ExcludeFromCodeCoverage]
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _apiHandler.TickEvent -= ApiHandlerOnTickEvent;
            if (_secondarySymbolAccount != null && _apiHandler.IsConnected())
                _apiHandler.UnsubscribePrice(_secondarySymbolAccount);
        }

        _disposed = true;
    }
}