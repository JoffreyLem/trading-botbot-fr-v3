using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class SuperTrend : BaseIndicator<SuperTrendResult>
{
    public SuperTrend()
    {
    }

    public SuperTrend(int loopBackPeriodRequested = 14)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 14;

    protected override IEnumerable<SuperTrendResult> Update(List<Candle> data)
    {
        return data.GetSuperTrend(LoopBackPeriod);
    }
}