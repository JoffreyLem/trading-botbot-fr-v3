using System.Text.Json.Serialization;

namespace robot_project_v3.Server.Dto.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PositionStateEnum
{
    Opened,
    Updated,
    Closed,
    Rejected
}