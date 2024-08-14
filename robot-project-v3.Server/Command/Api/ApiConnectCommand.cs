using robot_project_v3.Server.Dto.Response;

namespace robot_project_v3.Server.Command.Api;

public class ApiConnectCommand : CommandBaseApi<AcknowledgementResponse>
{
    public ConnectDto ConnectDto { get; set; }
}