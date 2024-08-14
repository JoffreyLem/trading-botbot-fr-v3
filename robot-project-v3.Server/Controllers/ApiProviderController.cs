using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_project_v3.Server.Dto.Response;
using robot_project_v3.Server.Services;

namespace robot_project_v3.Server.Controllers;

[Route("api/[controller]")]
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
    public async Task<IActionResult> IsConnected()
    {
        var connectionState = await apiProviderService.IsConnected();
        return Ok(new ApiResponse<bool>()
        {
            Data = connectionState,
        });
    }

    [HttpGet("typeHandler")]
    public async Task<IActionResult> GetTypeHandler()
    {
        var typeHandler = await apiProviderService.GetTypeProvider();
        return Ok(new ApiResponse<string>()
        {
            Data = typeHandler
        });
    }

    [HttpGet("listHandlers")]
    public async Task<IActionResult> GetListHandler()
    {
        var listHandlers = await apiProviderService.GetListProvider();
        return Ok(listHandlers);
    }

    [HttpGet("allSymbols")]
    public async Task<IActionResult> GetAllSymbol()
    {
        var allSymbols = await apiProviderService.GetAllSymbol();
        return Ok(allSymbols);
    }
    
}