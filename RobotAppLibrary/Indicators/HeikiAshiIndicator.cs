using System.Diagnostics.CodeAnalysis;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators;

[ExcludeFromCodeCoverage]
public sealed class HeikiAshiIndicator : BaseIndicator<HeikinAshiResult>
{

    public HeikiAshiIndicator()
    {
        
    }
    
    public HeikiAshiIndicator(int loopBackPeriodRequested)
    {
        LoopBackPeriod = loopBackPeriodRequested;
    }

    public override int LoopBackPeriod { get; set; } = 20;


    protected override IEnumerable<HeikinAshiResult> Update(List<Candle> data)
    {
        return data.GetHeikinAshi();
    }
}

public static class HeikiAshiIndicatorHelper
{
    public static bool IsStrongBuy(this HeikinAshiResult heiki)
    {
        if (heiki.Low == heiki.Open && heiki.Close >= heiki.Open) return true;

        return false;
    }

    public static bool IsBuy(this HeikinAshiResult heiki)
    {
        if (heiki.Close >= heiki.Open) return true;

        return false;
    }

    public static bool IsStrongSell(this HeikinAshiResult heiki)
    {
        if (heiki.High == heiki.Open && heiki.Close <= heiki.Open) return true;

        return false;
    }

    public static bool IsSell(this HeikinAshiResult heiki)
    {
        if (heiki.Close <= heiki.Open) return true;

        return false;
    }
}