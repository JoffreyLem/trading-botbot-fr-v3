namespace RobotAppLibrary.Modeles.Attribute;

public class TimeframeAttribute(Timeframe timeframe) : System.Attribute
{
    public Timeframe Timeframe { get; set; } = timeframe;
}