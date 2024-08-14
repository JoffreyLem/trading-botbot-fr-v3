using System.ComponentModel.DataAnnotations;

namespace robot_project_v3.Database.Modeles;

public class StrategyFile
{
    [Key] public int Id { get; set; }

    public string Name { get; set; }
    public string Version { get; set; }
    public byte[] Data { get; set; }
    public DateTime LastDateUpdate { get; set; }
}