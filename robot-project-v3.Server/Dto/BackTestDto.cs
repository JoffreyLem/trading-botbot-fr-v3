namespace robot_project_v3.Server.Dto;

public class BackTestDto
{
    public bool IsBackTestRunning { get; set; }

    public DateTime? LastBackTestExecution { get; set; }

    public GlobalResultsDto? ResultsBacktest { get; set; }
}