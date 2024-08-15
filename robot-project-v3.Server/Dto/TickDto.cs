namespace robot_project_v3.Server.Dto;

public struct TickDto
{
    public decimal? Ask { get; set; }
    public decimal? AskVolume { get; set; }
    public decimal? Bid { get; set; }
    public decimal? BidVolume { get; set; }
    public DateTime Date { get; set; }
    public decimal? Spread => Ask - Bid;
}