namespace robot_project_v3.Server.Dto.Response;

public class StrategyCompilationResponseDto
{
    public bool Compiled { get; set; }
    
    public StrategyFileDto? StrategyFileDto { get; set; }

    public List<string>? Errors { get; set; } = new();
}