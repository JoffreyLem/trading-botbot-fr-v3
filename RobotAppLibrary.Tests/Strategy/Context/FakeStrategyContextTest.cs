using RobotAppLibrary.Chart;
using RobotAppLibrary.Indicators;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.Strategy;

namespace RobotAppLibrary.Tests.Strategy.Context;

public class FakeStrategyContextTest : StrategyImplementationBase
{
    [Timeframe(Timeframe.OneMinute)] public BollingerBand BollingerBand;

    public BollingerBand BollingerBandtest;


    [Timeframe(Timeframe.Daily)] [MainChart]
    public IChart Chart1;

    [Timeframe(Timeframe.Monthly)] private IChart Chart2;

    [Timeframe(Timeframe.Weekly)] public IChart Chart3;

    public SarIndicator SarIndicator;

    [Timeframe(Timeframe.Weekly)] private SarIndicator SarIndicator2;

    [Timeframe(Timeframe.OneHour)] public SarIndicator SarIndicator3;

    [Timeframe(Timeframe.Weekly)] public SarIndicator SarIndicator4;


    [Timeframe(Timeframe.Monthly)] public SarIndicator SarIndicator5;

    public FakeStrategyContextTest()
    {
        RunOnTick = true;
        UpdateOnTick = true;
        CloseOnTick = true;
        DefaultSl = 10;
        DefaultTp = 10;
    }

    public override string? Version { get; } = "1-test";

    public bool ShouldUpdatePositionProperty { get; set; }

    public bool ShouldClosePositionProperty { get; set; }

    public override void Run()
    {
        RunEvent?.Invoke(this, EventArgs.Empty);
    }

    public override bool ShouldUpdatePosition(Position? position)
    {
        if (ShouldUpdatePositionProperty) return true;

        return false;
    }


    public override bool ShouldClosePosition(Position position)
    {
        if (ShouldClosePositionProperty) return true;

        return false;
    }

    public void OpenPositionForTest(TypeOperation typePosition, decimal sl, decimal tp, double volume = 0,
        double risk = 5)
    {
        OpenPosition(typePosition, sl, tp, volume, risk);
    }

    public event EventHandler? RunEvent;
}