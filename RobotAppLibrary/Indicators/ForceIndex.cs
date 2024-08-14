using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class ForceIndex : BaseIndicator<ForceIndexResult>
{
    public ForceIndex()
    {
        
    }
    public ForceIndex(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 20;

    protected override IEnumerable<ForceIndexResult> Update(List<Candle> data)
    {
        return data.GetForceIndex(LoopBackPeriod);
    }
}