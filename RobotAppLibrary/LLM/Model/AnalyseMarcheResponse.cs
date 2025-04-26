namespace RobotAppLibrary.LLM.Model;

public class AnalyseMarcheResponse
{
    public int ScoreSentiment { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
}