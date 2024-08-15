using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class Slope : BaseIndicator<SlopeResult>
{
    public Slope()
    {
    }

    public Slope(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 14;

    protected override IEnumerable<SlopeResult> Update(List<Candle> data)
    {
        return data.GetSlope(LoopBackPeriod);
    }
}