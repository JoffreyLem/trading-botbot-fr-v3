namespace RobotAppLibrary.LLM.Model;

public class AnalyseMarcheRequest
{
    public string? Description { get; set; }
    
    public List<Indicator> Indicators = new List<Indicator>();

    public List<Chart> Charts = new List<Chart>();

}