using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class Macd : BaseIndicator<MacdResult>
{
    public Macd(int fastPeriod, int slowPeriod, int signalPeriod)
    {
        FastPeriod = fastPeriod;
        SlowPeriod = slowPeriod;
        SignalPeriod = signalPeriod;
    }

    public int FastPeriod { get; set; } = 12;
    public int SlowPeriod { get; set; } = 26;
    public int SignalPeriod { get; set; } = 9;

    protected override IEnumerable<MacdResult> Update(List<Candle> data)
    {
        return data.GetMacd();
    }
}