using robot_project_v3.Database.Modeles;

namespace robot_project_v3.Server.Dto;

public class InitStrategyDto
{
    public StrategyFile StrategyFileDto { get; set; }
    public string Symbol { get; set; }
}