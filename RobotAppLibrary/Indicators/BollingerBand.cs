using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class BollingerBand : BaseIndicator<BollingerBandsResult>
{
    public BollingerBand()
    {
    }

    public BollingerBand(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 20;


    protected override IEnumerable<BollingerBandsResult> Update(List<Candle> data)
    {
        return data.GetBollingerBands(LoopBackPeriod);
    }
}