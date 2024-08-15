namespace robot_project_v3.Server.Dto;

public class StrategyInfoDto
{
    public string Id { get; set; }


    public string Symbol { get; set; }

    public string Timeframe { get; set; }

    public string Timeframe2 { get; set; }

    public string StrategyName { get; set; }

    public bool CanRun { get; set; }

    public bool StrategyDisabled { get; set; } = false;

    public bool SecureControlPosition { get; set; }

    public TickDto LastTick { get; set; } = new();

    public CandleDto LastCandle { get; set; } = new();
}