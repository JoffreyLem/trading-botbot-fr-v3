using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class CciIndicator : BaseIndicator<CciResult>
{
    public CciIndicator()
    {
        
    }
    
    public CciIndicator(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 20;


    protected override IEnumerable<CciResult> Update(List<Candle> data)
    {
        return data.GetCci(LoopBackPeriod);
    }
}