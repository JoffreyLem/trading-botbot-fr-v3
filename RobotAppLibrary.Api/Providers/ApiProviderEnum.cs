using System.Text.Json.Serialization;

namespace RobotAppLibrary.Api.Providers;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiProviderEnum
{
    Xtb = 0
}