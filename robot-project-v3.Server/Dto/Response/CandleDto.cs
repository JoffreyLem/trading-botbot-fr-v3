namespace robot_project_v3.Server.Dto.Response;

public class CandleDto
{
    public double BidVolume { get; set; }
    public double AskVolume { get; set; }
    public DateTime Date { get; set; }

    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public double Volume { get; set; }
}