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
public class StrategyController(IStrategyService strategyService) : ControllerBase
{
    [HttpPost("init")]
    public async Task<IActionResult> InitStrategy([FromBody] StrategyInitDto strategyInitDto)
    {
        await strategyService.InitStrategy(strategyInitDto);
        return Ok();
    }


    [HttpGet("all")]
    [ProducesResponseType(typeof(List<StrategyInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllStrategy()
    {
        var data = await strategyService.GetAllStrategy();

        return Ok(data);
    }

    [HttpPost("close/{id}")]
    public async Task<IActionResult> CloseStrategy(string id)
    {
        await strategyService.CloseStrategy(id);
        return Ok();
    }

    [HttpGet("{id}/info")]
    [ProducesResponseType(typeof(StrategyInfoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StrategyInfoDto>> GetStrategyInfo(string id)
    {
        return await strategyService.GetStrategyInfo(id);
    }

    [HttpGet("{id}/result")]
    [ProducesResponseType(typeof(GlobalResultsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResult(string id)
    {
        return Ok(await strategyService.GetResult(id));
    }


    [HttpPost("{id}/canrun")]
    public async Task<IActionResult> SetCanRun(string id, [FromQuery] bool value)
    {
        await strategyService.SetCanRun(id, value);
        return Ok();
    }

    [HttpGet("{id}/positions/opened")]
    [ProducesResponseType(typeof(List<PositionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpenedPositions(string id)
    {
        return Ok(await strategyService.GetOpenedPositions(id));
    }

    [HttpPost("runBacktest/{id}")]
    public async Task<ActionResult<BackTestDto>> RunBackTest(string id,
        [FromBody] BackTestRequestDto backTestRequestDto)
    {
        return await strategyService.RunBackTest(id, backTestRequestDto);
    }


    [HttpGet("{id}/resultBacktest")]
    public async Task<ActionResult<BackTestDto>> GetResultBacktest(string id)
    {
        return await strategyService.GetBacktestResult(id);
    }
}