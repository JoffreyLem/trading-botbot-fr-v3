using System.ComponentModel.DataAnnotations;

namespace robot_project_v3.Server.Dto;

public class StrategyInitDto
{
    [Required] public string StrategyFileId { get; set; }
    [Required] public string Symbol { get; set; }
}