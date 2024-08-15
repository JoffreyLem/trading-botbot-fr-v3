using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_project_v3.Server.Dto;
using robot_project_v3.Server.Services;

namespace robot_project_v3.Server.Controllers;

[Route("api/[controller]")]
[ProducesResponseType(typeof(ApiResponseError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponseError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponseError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponseError), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponseError), StatusCodes.Status500InternalServerError)]
[ApiController]
[Authorize]
public class ApiProviderController(IApiProviderService apiProviderService) : ControllerBase
{
    [HttpPost("connect")]
    public async Task<IActionResult> Connect([FromBody] ConnectDto connectDto)
    {
        await apiProviderService.Connect(connectDto);
        return Ok();
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect()
    {
        await apiProviderService.Disconnect();
        return Ok();
    }

    [HttpGet("isConnected")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsConnected()
    {
        var connectionState = await apiProviderService.IsConnected();
        return Ok(new ApiResponse<bool>
        {
            Data = connectionState
        });
    }

    [HttpGet("typeHandler")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypeHandler()
    {
        var typeHandler = await apiProviderService.GetTypeProvider();
        return Ok(new ApiResponse<string>
        {
            Data = typeHandler
        });
    }

    [HttpGet("listHandlers")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListHandler()
    {
        var listHandlers = await apiProviderService.GetListProvider();
        return Ok(listHandlers);
    }

    [HttpGet("allSymbols")]
    [ProducesResponseType(typeof(List<SymbolInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSymbol()
    {
        var allSymbols = await apiProviderService.GetAllSymbol();
        return Ok(allSymbols);
    }
}