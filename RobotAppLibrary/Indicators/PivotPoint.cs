using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class PivotPoint : BaseIndicator<PivotPointsResult>
{
    public PivotPoint()
    {
        
    }
    
    public PivotPoint(PeriodSize periodSize, PivotPointType type )
    {
        PeriodSize = periodSize;
        PivotPointType = type;
    }

    public PeriodSize PeriodSize { get; set; } = PeriodSize.Day;
    public PivotPointType PivotPointType { get; set; } = PivotPointType.Standard;

    protected override IEnumerable<PivotPointsResult> Update(List<Candle> data)
    {
        return data.GetPivotPoints(PeriodSize, PivotPointType);
    }
}