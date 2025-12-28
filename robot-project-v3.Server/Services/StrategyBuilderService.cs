using AutoMapper;
using robot_project_v3.Database.Modeles;
using robot_project_v3.Database.Repositories;
using robot_project_v3.Server.Dto;
using robot_project_v3.Server.Exceptions;
using RobotAppLibrary.StrategyDynamicCompiler;
using ILogger = Serilog.ILogger;

namespace robot_project_v3.Server.Services;

public interface IStrategyBuilderService
{
    Task<StrategyCompilationResponseDto> CreateNewStrategy(string data);

    Task<StrategyFileDto> GetStrategyFile(string id);

    Task<List<StrategyFileDto>> GetAllStrategyFile();

    Task DeleteStrategyFile(string id);

    Task<StrategyCompilationResponseDto> UpdateStrategyFile(string id, string data);
}

public class StrategyBuilderService(ILogger logger, IMapper mapper, IStrategyFileRepository strategyFileRepository)
    : IStrategyBuilderService
{
    private readonly ILogger _logger = logger.ForContext<StrategyBuilderService>();


    public async Task<StrategyCompilationResponseDto> CreateNewStrategy(string data)
    {
        var strategyCreateRsp = new StrategyCompilationResponseDto();
        try
        {
            var compiledCode = StrategyDynamicCompiler.TryCompileSourceCode(data);
            
            var strategyFile = new StrategyFile
            {
                Data = data, Name = compiledCode.name, Version = compiledCode.version,
                LastDateUpdate = DateTime.UtcNow
            };

            await strategyFileRepository.AddAsync(strategyFile);

            strategyCreateRsp.Compiled = true;
            strategyCreateRsp.StrategyFileDto = new StrategyFileDto
            {
                Id = strategyFile.Id,
                Data = strategyFile.Data,
                LastDateUpdate = strategyFile.LastDateUpdate,
                Name = strategyFile.Name,
                Version = strategyFile.Version
            };

        
        }
        catch (CompilationException e)
        {
            strategyCreateRsp.Compiled = false;
            strategyCreateRsp.Errors = e.CompileErrors.Select(e => e.ToString()).ToList();
        }
        catch (Exception e)
        {
            _logger?.Error(e, "An exception occurred while creating the new strategy");
            throw new ApiException("An exception occurred while creating the new strategy");
        }

        return strategyCreateRsp;
    }

    public async Task<StrategyFileDto> GetStrategyFile(string id)
    {
        try
        {
            var data = await strategyFileRepository.GetByIdAsync(id);

            return new StrategyFileDto
            {
                Id = data.Id,
                Data = data.Data,
                LastDateUpdate = data.LastDateUpdate,
                Name = data.Name,
                Version = data.Version
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, "Can't get all strategy file in db");
            throw new ApiException("Can't get all strategy file in db");
        }
    }

    public async Task<StrategyCompilationResponseDto> UpdateStrategyFile(string id, string data)
    {
        var strategyCreateRsp = new StrategyCompilationResponseDto();
        try
        {
            
            var compiledCode = StrategyDynamicCompiler.TryCompileSourceCode(data);

            var strategyFileSelected = await strategyFileRepository.GetByIdAsync(id);

            strategyFileSelected.Version = compiledCode.version;
            strategyFileSelected.LastDateUpdate = DateTime.UtcNow;
            strategyFileSelected.Data = data;

            await strategyFileRepository.UpdateAsync(strategyFileSelected);

            strategyCreateRsp.Compiled = true;
            strategyCreateRsp.StrategyFileDto = new StrategyFileDto
            {
                Id = strategyFileSelected.Id,
                Data = strategyFileSelected.Data,
                LastDateUpdate = strategyFileSelected.LastDateUpdate,
                Name = strategyFileSelected.Name,
                Version = strategyFileSelected.Version
            };

        }
        catch (CompilationException e)
        {
            strategyCreateRsp.Compiled = false;
            strategyCreateRsp.Errors = e.CompileErrors.Select(e => e.ToString()).ToList();
        }
        catch (Exception e) when (e is not CompilationException)
        {
            _logger?.Error(e, "An exception occurred while updating strategyfile {id}", id);
            throw new ApiException($"An error occured while updating strategy {id}");
        }

        return strategyCreateRsp;
    }

    public async Task<List<StrategyFileDto>> GetAllStrategyFile()
    {
        try
        {
            var data = await strategyFileRepository.GetAllAsync();

            return data.Select(x => new StrategyFileDto
            {
                Id = x.Id,
                Version = x.Version,
                Name = x.Name,
                LastDateUpdate = x.LastDateUpdate
            }).ToList();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Can't get all strategy file in db");
            throw new Exception();
        }
    }

    public async Task DeleteStrategyFile(string id)
    {
        try
        {
            await strategyFileRepository.DeleteAsync(id);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Can't delete strategy {id}", id);
            throw new Exception();
        }
    }
}