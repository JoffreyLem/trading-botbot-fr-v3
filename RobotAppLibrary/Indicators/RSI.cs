using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class Rsi : BaseIndicator<RsiResult>
{

    public Rsi()
    {
        
    }
    
    public Rsi(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 14;

    protected override IEnumerable<RsiResult> Update(List<Candle> data)
    {
        return data.GetRsi(LoopBackPeriod);
    }
}