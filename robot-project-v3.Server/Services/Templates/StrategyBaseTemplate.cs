using RobotAppLibrary.Chart;
using RobotAppLibrary.Indicators;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.Strategy;

namespace robot_project_v3.Server.Services.Templates;

public class StrategyBaseTemplate : StrategyImplementationBase
{
    public StrategyBaseTemplate()
    {
        RunOnTick = true;
        CloseOnTick = true;
    }
    
    [Timeframe(Timeframe.FifteenMinutes)] [MainChart]
    public IChart Chart;

    public SarIndicator SarIndicator { get; set; } = new();

    public override string? Version => "test";

    public override void Run()
    {
        var type = SarIndicator.IsBuy() ? TypeOperation.Buy : TypeOperation.Sell;
        var sl = SarIndicator.Last().Sar;

        OpenPosition(type, CalculateStopLoss(100, type), CalculateTakeProfit(80, type));
    }


    public override bool ShouldClosePosition(Position position)
    {
        return true;
    }
}