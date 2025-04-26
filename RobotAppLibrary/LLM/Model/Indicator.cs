using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.LLM.Model;

public class Indicator
{
    public string Name { get; set; }

    public Timeframe Timeframe { get; set; }
    
    public List<dynamic> Values { get; set; }
}