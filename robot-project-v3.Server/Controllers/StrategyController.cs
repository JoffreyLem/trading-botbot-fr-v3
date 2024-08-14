using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_project_v3.Server.Dto.Request;
using robot_project_v3.Server.Dto.Response;
using robot_project_v3.Server.Services;

namespace robot_project_v3.Server.Controllers;

[Route("api/[controller]")]
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

    [HttpGet("timeframes")]
    public async Task<ActionResult<List<string>>> GetListTimeframes()
    {
        return await strategyService.GetListTimeframes();
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<StrategyInfoDto>>> GetAllStrategy()
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
    public async Task<ActionResult<StrategyInfoDto>> GetStrategyInfo(string id)
    {
        return await strategyService.GetStrategyInfo(id);
    }

    [HttpGet("{id}/result")]
    public async Task<ActionResult<GlobalResultsDto>> GetResult(string id)
    {
        return await strategyService.GetResult(id);
    }


    [HttpPost("{id}/canrun")]
    public async Task<IActionResult> SetCanRun(string id, [FromQuery] bool value)
    {
        await strategyService.SetCanRun(id, value);
        return Ok();
    }

    [HttpGet("{id}/positions/opened")]
    public async Task<ActionResult<List<PositionDto>>> GetOpenedPositions(string id)
    {
        return await strategyService.GetOpenedPositions(id);
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