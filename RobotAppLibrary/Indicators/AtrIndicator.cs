using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class AtrIndicator : BaseIndicator<AtrResult>
{

    public AtrIndicator()
    {
        
    }

    public AtrIndicator(int loopBackPeriod)
    {
        LoopBackPeriod = loopBackPeriod;
    }
    
    public sealed override int LoopBackPeriod { get; set; } = 14;
    
    protected override IEnumerable<AtrResult> Update(List<Candle> data)
    {
        return data.GetAtr(LoopBackPeriod);
    }
}