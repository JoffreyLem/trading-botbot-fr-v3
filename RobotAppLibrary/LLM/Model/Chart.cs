using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.LLM.Model;

public class Chart
{
    public Timeframe Timeframe { get; set; }
    
    public IEnumerable<Candle> Values { get; set; }
}