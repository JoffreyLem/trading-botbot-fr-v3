using System.Runtime.CompilerServices;
using RobotAppLibrary.Modeles;
using Serilog;

[assembly: InternalsVisibleTo("RobotAppLibrary.Tests")]

namespace RobotAppLibrary.Strategy;

public abstract class StrategyImplementationBase
{
    internal Func<decimal, TypeOperation, decimal> CalculateStopLossFunc = null!;
    internal Func<decimal, TypeOperation, decimal> CalculateTakeProfitFunc = null!;
    internal Func<TypeOperation, decimal,decimal, double, double, long,Task> OpenPositionAction = null!;
    public ILogger? Logger;
    public virtual string Name => GetType().Name;
    public abstract string? Version { get; }
    public bool RunOnTick { get; set; } 
    public bool UpdateOnTick { get; set; } 
    public bool CloseOnTick { get; set; } 
    
    protected internal int DefaultSl { get; set; }
    protected internal int DefaultTp { get; set; }

    public abstract void Run();

    protected void OpenPosition(TypeOperation typePosition, decimal sl = 0,
        decimal tp = 0, double volume = 0, double risk = 0, long expiration = 0)
    {
        OpenPositionAction.Invoke(typePosition, sl, tp, risk, volume, expiration).GetAwaiter().GetResult();
    }

    public decimal CalculateStopLoss(decimal pips, TypeOperation typePosition)
    {
        return CalculateStopLossFunc.Invoke(pips, typePosition);
    }

    public decimal CalculateTakeProfit(decimal pips, TypeOperation typePosition)
    {
        return CalculateTakeProfitFunc.Invoke(pips, typePosition);
    }
    
    public virtual bool ShouldUpdatePosition(Position position)
    {
        return false;
    }

    public virtual bool ShouldClosePosition(Position position)
    {
        return false;
    }
}