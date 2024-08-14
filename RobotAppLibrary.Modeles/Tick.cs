namespace RobotAppLibrary.Modeles;

public record Tick(
    decimal? Ask,
    decimal? AskVolume,
    decimal? Bid,
    decimal? BidVolume,
    DateTime Date,
    string Symbol
)
{
 
    public Tick() : this(null, null, null, null, default, string.Empty) { }
    public decimal? Spread => Ask - Bid;

    public override string ToString() =>
        $"Symbol: {Symbol}\n" +
        $"Date: {Date}\n" +
        $"Ask: {Ask}\n" +
        $"Bid: {Bid}\n" +
        $"AskVolume: {AskVolume}\n" +
        $"BidVolume: {BidVolume}\n" +
        $"Spread: {Spread}";
}
