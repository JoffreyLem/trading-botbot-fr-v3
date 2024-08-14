using RobotAppLibrary.Chart;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.Strategy;

namespace RobotAppLibrary.Tests.Strategy.Context;

public class FakeStrategyContextChartWithNoMainChart : StrategyImplementationBase
{
    public override string? Version { get; }
    public override void Run()
    {
        throw new NotImplementedException();
    }
}


public class FakeStrategyContextChartWithNoTimeframeAttribute : StrategyImplementationBase
{

    [MainChart] public IChart Chart;
    
    public override string? Version { get; }
    public override void Run()
    {
        throw new NotImplementedException();
    }
}