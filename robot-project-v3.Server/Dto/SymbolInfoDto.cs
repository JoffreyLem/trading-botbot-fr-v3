namespace robot_project_v3.Server.Dto;

public class SymbolInfoDto
{
    public string? Category { get; set; }
    public long? ContractSize { get; set; }

    public string? Currency { get; set; }

    public string? CurrencyProfit { get; set; }
    public double? LotMin { get; set; }
    public int Precision { get; set; }
    public required string Symbol { get; set; }
    public required double TickSize { get; set; }

    public double Leverage { get; set; }
}