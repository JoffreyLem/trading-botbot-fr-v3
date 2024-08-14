using System.ComponentModel.DataAnnotations;
using robot_project_v3.Server.Dto.Enum;
using RobotAppLibrary.Modeles;

namespace robot_project_v3.Server.Dto.Request;

public class StrategyInitDto
{
    [Required] public string StrategyFileId { get; set; }
    [Required] public string Symbol { get; set; }

    [Required] public Timeframe Timeframe { get; set; }
    [Required] public Timeframe Timeframe2 { get; set; }
}