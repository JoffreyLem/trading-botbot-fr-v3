using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public class SarIndicator : BaseIndicator<ParabolicSarResult>
{

    public SarIndicator()
    {
        
    }
    
    public SarIndicator(double accelerationStep = 0.02, double maxAccelerationFactor = 0.2)
    {
        AccelerationStep = accelerationStep;
        MaxAccelerationFactor = maxAccelerationFactor;
    }

    public double AccelerationStep { get; init; } = 0.02;

    public double MaxAccelerationFactor { get; init; } = 0.2;

    protected override IEnumerable<ParabolicSarResult> Update(List<Candle> data)
    {
        return data.GetParabolicSar(AccelerationStep, MaxAccelerationFactor);
    }

    public bool IsBuy()
    {
        return Last().Sar < (double?)LastTick.Bid;
    }

    public bool IsSell()
    {
        return Last().Sar > (double?)LastTick.Bid;
    }
}