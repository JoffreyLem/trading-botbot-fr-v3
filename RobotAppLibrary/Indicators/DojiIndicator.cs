using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class DojiIndicator : BaseIndicator<CandleResult>
{
    protected override IEnumerable<CandleResult> Update(List<Candle> data)
    {
        return data.GetDoji();
    }
}