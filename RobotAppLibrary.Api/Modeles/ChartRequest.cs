using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Modeles;

public class ChartRequest
{
    public Timeframe Timeframe { get; set; }
    public DateTime Start { get; set; } = new DateTime();
    public DateTime? End { get; set; }
    public string Symbol { get; set; }
    
}