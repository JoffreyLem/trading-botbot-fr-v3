namespace RobotAppLibrary.Modeles;

public sealed class SymbolInfo
{
    public Category Category { get; set; }
    public long? ContractSize { get; set; }

    /// <summary>
    ///     Base Currency
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    ///     Currency profit
    /// </summary>
    public string? CurrencyProfit { get; set; }

    public double? LotMin { get; set; }
    public int Precision { get; set; }
    public required string Symbol { get; set; }
    public required double TickSize { get; set; }

    /// <summary>
    ///     In percentage
    /// </summary>
    public double Leverage { get; set; }
}

public enum Category
{
    Forex,
    Indices,
    Stock,
    Commodities,
    Unknow,
    Crypto,
    ExchangeTradedFund
}