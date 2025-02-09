using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class AdxIndicator : BaseIndicator<AdxResult>
{
    
    public override int LoopBackPeriod { get; set; } = 14;

    protected override IEnumerable<AdxResult> Update(List<Candle> data)
    {
        return data.GetAdx(LoopBackPeriod);
    }
}