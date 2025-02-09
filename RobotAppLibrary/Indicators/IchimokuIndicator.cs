using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class IchimokuIndicator : BaseIndicator<IchimokuResult>
{
    
    public int TenkanPeriods { get; set; } = 9;
    public int KijunPeriods { get; set; } = 26;
    public int SenkouBPeriods { get; set; } = 52;
    public int OffsetPeriods { get; set; }
    public int SenkouOffset { get; set; }
    public int ChikouOffset { get; set; }
    protected override IEnumerable<IchimokuResult> Update(List<Candle> data)
    {
        return data.GetIchimoku();
    }
}