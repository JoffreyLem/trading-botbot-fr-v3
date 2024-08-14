using Skender.Stock.Indicators;

namespace RobotAppLibrary.Modeles;

public class Candle : CandleProperties
{
    public List<Tick> Ticks { get; set; } = new List<Tick>();
    public decimal BidVolume { get; set; }
    public decimal AskVolume { get; set; }
    
    public override string ToString()
    {
        return $"Date: {Date:yyyy-MM-dd HH:mm:ss}\n" +
               $"Open: {Open}\n" +
               $"High: {High}\n" +
               $"Low: {Low}\n" +
               $"Close: {Close}\n" +
               $"Volume: {Volume}\n" +
               $"BidVolume: {BidVolume}\n" +
               $"AskVolume: {AskVolume}\n";
    }
}