namespace robot_project_v3.Server.Modele;

public class LogCommand
{
    public string LogCommandName { get; set; }
    
    public string? StrategyId { get; set; }
    public object? CommandData { get; set; }
    
    public object? CommandResponse { get; set; }
}