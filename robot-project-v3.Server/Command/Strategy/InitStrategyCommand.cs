using robot_project_v3.Database.Modeles;

namespace robot_project_v3.Server.Command.Strategy;

public class InitStrategyCommand : CommandBaseStrategy<AcknowledgementResponse>
{
    public StrategyFile StrategyFileDto { get; set; }
    public string Symbol { get; set; }
}