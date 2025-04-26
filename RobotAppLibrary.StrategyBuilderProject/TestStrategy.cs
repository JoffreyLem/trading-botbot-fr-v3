using RobotAppLibrary.Chart;
using RobotAppLibrary.Indicators;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.Strategy;

public class TestStrategy : StrategyImplementationBase
{
    public TestStrategy()
    {
        RunOnTick = true;
        CloseOnTick = true;
    }
    
    [Timeframe(Timeframe.FifteenMinutes)] [MainChart]
    public IChart Chart;

    public SarIndicator SarIndicator  = new();

    public override string? Version => "1.0";

    public override void Run()
    {
        var type = SarIndicator.IsBuy() ? TypeOperation.Buy : TypeOperation.Sell;
        var sl = SarIndicator.Last().Sar;

        OpenPosition(type, CalculateStopLoss(100, type), CalculateTakeProfit(80, type));
    }


    public override bool ShouldClosePosition(Position position)
    {
        return false;
    }
}