using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace robot_project_v3.Database.Modeles;

public class StrategyFile
{
    [Key]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Data { get; set; }
    public DateTime LastDateUpdate { get; set; }
}