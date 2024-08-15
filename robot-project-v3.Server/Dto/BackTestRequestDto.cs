namespace robot_project_v3.Server.Dto;

public class BackTestRequestDto
{
    public double Balance { get; set; }
    public decimal MinSpread { get; set; }
    public decimal MaxSpread { get; set; }
}