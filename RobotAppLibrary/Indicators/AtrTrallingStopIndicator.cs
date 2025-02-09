using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class AtrTrallingStopIndicator : BaseIndicator<AtrStopResult>
{
    public double Multiplier { get; set; } = 3;

    public EndType EndType { get; set; } = EndType.Close;

    public override int LoopBackPeriod { get; set; } = 21;
    protected override IEnumerable<AtrStopResult> Update(List<Candle> data)
    {
        return data.GetAtrStop(LoopBackPeriod, Multiplier, EndType);
    }
}