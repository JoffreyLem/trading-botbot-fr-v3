using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class FractalBandIndicator : BaseIndicator<FcbResult>
{
    public FractalBandIndicator()
    {
    }

    public FractalBandIndicator(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 20;

    protected override IEnumerable<FcbResult> Update(List<Candle> data)
    {
        return data.GetFcb(LoopBackPeriod);
    }
}