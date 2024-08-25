using System.ComponentModel;

namespace RobotAppLibrary.Modeles;

public class Position
{
    public Position()
    {
    }

    public Position(string? strategyId, string? symbol, TypeOperation typePosition, decimal? spread, decimal openPrice,
        decimal stopLoss, decimal takeProfit, double volume)
    {
        Id = Guid.NewGuid().ToString();
        StrategyId = strategyId;
        Symbol = symbol;
        TypePosition = typePosition;
        Spread = spread;
        OpenPrice = openPrice;
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        Volume = volume;
    }

    public string? Id { get; set; }
    public string? StrategyId { get; set; }
    public string PositionStrategyReferenceId => $"{StrategyId}|{Id}";
    public string? Order { get; set; }
    public string? Symbol { get; set; }
    public TypeOperation TypePosition { get; set; }
    public decimal? Spread { get; set; }
    public decimal Profit { get; set; }
    public decimal OpenPrice { get; set; }
    public DateTime DateOpen { get; set; }
    public decimal? ClosePrice { get; set; }
    
    // TODO : Test
    public decimal? CurrentPrice { get; set; }
    public DateTime? DateClose { get; set; }
    public ReasonClosed? ReasonClosed { get; set; } = null;
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public double Volume { get; set; }
    public decimal Pips => ClosePrice != 0 ? Math.Abs(OpenPrice - ClosePrice.GetValueOrDefault()) : 0;
    public StatusPosition StatusPosition { get; set; }
    public bool Opened { get; set; }

    public Position? Clone()
    {
        return new Position
        {
            Id = Id,
            StrategyId = StrategyId,
            Symbol = Symbol,
            TypePosition = TypePosition,
            Spread = Spread,
            OpenPrice = OpenPrice,
            StopLoss = StopLoss,
            TakeProfit = TakeProfit,
            Volume = Volume,
            Order = Order,
            Profit = Profit,
            DateOpen = DateOpen,
            ClosePrice = ClosePrice,
            DateClose = DateClose,
            ReasonClosed = ReasonClosed,
            StatusPosition = StatusPosition,
            Opened = Opened
        };
    }
}

public enum ReasonClosed
{
    Sl,
    Tp,
    Margin,
    Closed
}

public enum StatusPosition
{
    Open,
    Updated,
    Accepted,
    Pending,
    Close,
    Rejected,
    Unknow
}

public enum TypeOperation
{
    [Description("Buy")] Buy = 0,
    [Description("Sell")] Sell = 1,
    [Description("None")] None = 8
}