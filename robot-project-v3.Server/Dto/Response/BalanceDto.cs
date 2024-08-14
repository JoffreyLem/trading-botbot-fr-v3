namespace robot_project_v3.Server.Dto.Response;

public class AccountBalanceDto
{
    public double? MarginLevel { get; set; }

    public double? MarginFree { get; set; }

    public double? Margin { get; set; }

    public double? Equity { get; set; }

    public double? Credit { get; set; }

    public double? Balance { get; set; }
}