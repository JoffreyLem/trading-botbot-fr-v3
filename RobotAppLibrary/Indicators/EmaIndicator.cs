using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class EmaIndicator : BaseIndicator<EmaResult>
{
    public EmaIndicator()
    {
    }

    public EmaIndicator(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 20;

    protected override IEnumerable<EmaResult> Update(List<Candle> data)
    {
        return data.GetEma(LoopBackPeriod);
    }
}