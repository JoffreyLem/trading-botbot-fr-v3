using robot_project_v3.Server.Dto;

namespace robot_project_v3.Server.BackgroundService.Command.Api;

public class GetAllSymbolCommand : CommandBaseApi<List<SymbolInfoDto>, EmptyCommand>
{
}