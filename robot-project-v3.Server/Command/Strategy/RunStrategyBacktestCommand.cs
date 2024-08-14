using robot_project_v3.Server.Dto.Response;

namespace robot_project_v3.Server.Command.Strategy;

// TODO : Penser à changer ici
public class RunStrategyBacktestCommand : CommandBaseStrategy<BackTestDto>
{
    public double Balance { get; set; }

    public decimal MinSpread { get; set; }

    public decimal MaxSpread { get; set; }
}