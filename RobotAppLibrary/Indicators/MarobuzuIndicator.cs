using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class MarobuzuIndicator : BaseIndicator<CandleResult>
{
    protected override IEnumerable<CandleResult> Update(List<Candle> data)
    {
        return data.GetMarubozu();
    }
}