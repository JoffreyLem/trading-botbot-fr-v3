using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_project_v3.Server.Dto.Response;
using robot_project_v3.Server.Services;

namespace robot_project_v3.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StrategyBuilderController(IStrategyBuilderService strategyBuilderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateNewStrategy([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fichier vide ou non fourni.");
        }

        string content;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            content = await reader.ReadToEndAsync();
        }

        var result = await strategyBuilderService.CreateNewStrategy(content);
        return Ok(result);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllStrategyFile()
    {
        var strategies = await strategyBuilderService.GetAllStrategyFile();
        return Ok(strategies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStrategy(int id)
    {
        var strategies = await strategyBuilderService.GetStrategyFile(id);
        return Ok(strategies);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStrategyFile(int id)
    {
        await strategyBuilderService.DeleteStrategyFile(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStrategyFile([FromRoute] int id, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fichier vide ou non fourni.");
        }

        string content;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            content = await reader.ReadToEndAsync();
        }
        var updatedStrategy = await strategyBuilderService.UpdateStrategyFile(id, content);
        return Ok(updatedStrategy);
    }


    [HttpGet("GetTemplate")]
    public async Task<IActionResult> GetTemplate()
    {
        var filePath = "Services/Templates/StrategyBaseTemplate.cs";
        if (System.IO.File.Exists(filePath))
        {
            var content = await System.IO.File.ReadAllTextAsync(filePath);
            return Ok(new StrategyFileDto
            {
                Data = content,
                Name = "StrategyBaseTemplate"
            });
        }

        return NotFound("Template file not found.");
    }
}