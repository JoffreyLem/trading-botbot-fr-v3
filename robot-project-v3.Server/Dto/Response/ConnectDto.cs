using System.ComponentModel.DataAnnotations;
using RobotAppLibrary.Api.Providers;

namespace robot_project_v3.Server.Dto.Response;

public class ConnectDto
{
    public string? User { get; set; }


    public string? Pwd { get; set; }

    [Required] public ApiProviderEnum? HandlerEnum { get; set; }
}