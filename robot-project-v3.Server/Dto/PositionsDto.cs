using System.ComponentModel;

namespace robot_project_v3.Server.Dto;

public class PositionDto
{
    public string? Id { get; set; }
    public string? Symbol { get; set; }
    public string? TypePosition { get; set; }
    public double? Spread { get; set; }
    public decimal? Profit { get; set; }
    public decimal? OpenPrice { get; set; }
    public DateTime DateOpen { get; set; }
    public decimal? ClosePrice { get; set; }
    public DateTime? DateClose { get; set; }
    public string? ReasonClosed { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public double? Volume { get; set; }
    public decimal? Pips { get; set; }
    public string? StatusPosition { get; set; }
    public string Comment { get; set; }
}