using RobotAppLibrary.Indicators;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Strategy;

namespace robot_project_v3.Server.Services.Templates;

public class StrategyBaseTemplate : StrategyImplementationBase
{
    public StrategyBaseTemplate()
    {
        RunOnTick = true;
        CloseOnTick = true;
    }

    public SarIndicator SarIndicator { get; set; } = new();

    public override string? Version => "7";

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