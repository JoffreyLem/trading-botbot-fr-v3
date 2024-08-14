using System.Text;
using AutoMapper;
using robot_project_v3.Database.Modeles;
using robot_project_v3.Database.Repositories;
using robot_project_v3.Server.Dto.Response;
using robot_project_v3.Server.Exceptions;
using RobotAppLibrary.StrategyDynamicCompiler;
using ILogger = Serilog.ILogger;

namespace robot_project_v3.Server.Services;

public interface IStrategyBuilderService
{
    Task<StrategyCompilationResponseDto> CreateNewStrategy(string data);

    Task<StrategyFileDto> GetStrategyFile(int id);

    Task<List<StrategyFileDto>> GetAllStrategyFile();

    Task DeleteStrategyFile(int id);

    Task<StrategyCompilationResponseDto> UpdateStrategyFile(int id, string data);
}

public class StrategyBuilderService(ILogger logger, IMapper mapper, IStrategyFileRepository strategyFileRepository)
    : IStrategyBuilderService
{
    private readonly ILogger _logger = logger.ForContext<StrategyBuilderService>();
    private readonly IMapper _mapper = mapper;
    private readonly IStrategyFileRepository _strategyFileRepository = strategyFileRepository;


      public async Task<StrategyCompilationResponseDto> CreateNewStrategy(string data)
    {
        var strategyCreateRsp = new StrategyCompilationResponseDto();
        try
        {
            var sourceCode = data;
            var compiledCode = StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode);

            var context = new CustomLoadContext();
            using var stream = new MemoryStream(compiledCode);
            var assembly = context.LoadFromStream(stream);

            var className = StrategyDynamiqCompiler.GetFirstClassName(sourceCode);

            if (string.IsNullOrEmpty(className))
            {
                strategyCreateRsp.Compiled = false;
                strategyCreateRsp.Errors.Add("Class name not found in file");
                return strategyCreateRsp;
            }

            var type = assembly.GetType(className);
            var instance = Activator.CreateInstance(type);

            var nameValue = (string)type.GetProperty("Name")?.GetValue(instance);
            var versionValue = (string)type.GetProperty("Version")?.GetValue(instance);

            if (string.IsNullOrEmpty(nameValue) || string.IsNullOrEmpty(versionValue))
            {
                strategyCreateRsp.Compiled = false;
                strategyCreateRsp.Errors.Add("Name or version not found");
                return strategyCreateRsp;
            }

            var strategyFile = new StrategyFile
            {
                Data = Encoding.UTF8.GetBytes(data), Name = nameValue, Version = versionValue,
                LastDateUpdate = DateTime.UtcNow
            };

            await _strategyFileRepository.AddAsync(strategyFile);

            strategyCreateRsp.Compiled = true;
            strategyCreateRsp.StrategyFileDto = new StrategyFileDto()
            {
                Id = strategyFile.Id,
                Data = Encoding.UTF8.GetString(strategyFile.Data),
                LastDateUpdate = strategyFile.LastDateUpdate,
                Name = strategyFile.Name,
                Version = strategyFile.Version
            };

            if (instance is IDisposable disposable) disposable.Dispose();

            context.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
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

    public async Task<StrategyFileDto> GetStrategyFile(int id)
    {
        try
        {
            var data = await _strategyFileRepository.GetByIdAsync(id);

            return new StrategyFileDto
            {
                Id = data.Id,
                Data = Encoding.UTF8.GetString(data.Data),
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

   public async Task<StrategyCompilationResponseDto> UpdateStrategyFile(int id, string data)
    {
        var strategyCreateRsp = new StrategyCompilationResponseDto();
        try
        {
            var sourceCode = data;

            var compiledCode = StrategyDynamiqCompiler.TryCompileSourceCode(sourceCode);


            var context = new CustomLoadContext();
            using var stream = new MemoryStream(compiledCode);
            var assembly = context.LoadFromStream(stream);

            var className = StrategyDynamiqCompiler.GetFirstClassName(sourceCode);
            if (string.IsNullOrEmpty(className))
            {
                strategyCreateRsp.Compiled = false;
                strategyCreateRsp.Errors.Add("Class name not found in file");
                return strategyCreateRsp;
            }

            var type = assembly.GetType(className);
            var instance = Activator.CreateInstance(type);

            var nameValue = (string)type.GetProperty("Name")?.GetValue(instance);
            var versionValue = (string)type.GetProperty("Version")?.GetValue(instance);

            if (string.IsNullOrEmpty(nameValue) || string.IsNullOrEmpty(versionValue))
            {
                strategyCreateRsp.Compiled = false;
                strategyCreateRsp.Errors.Add("Name or version not found");
                return strategyCreateRsp;
            }

            var strategyFileSelected = await _strategyFileRepository.GetByIdAsync(id);

            strategyFileSelected.Name = nameValue;
            strategyFileSelected.Version = versionValue;
            strategyFileSelected.LastDateUpdate = DateTime.UtcNow;
            strategyFileSelected.Data = Encoding.UTF8.GetBytes(sourceCode);

            await _strategyFileRepository.UpdateAsync(strategyFileSelected);

            strategyCreateRsp.Compiled = true;
            strategyCreateRsp.StrategyFileDto = new StrategyFileDto()
            {
                Id = strategyFileSelected.Id,
                Data = Encoding.UTF8.GetString(strategyFileSelected.Data),
                LastDateUpdate = strategyFileSelected.LastDateUpdate,
                Name = strategyFileSelected.Name,
                Version = strategyFileSelected.Version
            };


            if (instance is IDisposable disposable) disposable.Dispose();

            context.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
            var data = await _strategyFileRepository.GetAllAsync();

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

    public async Task DeleteStrategyFile(int id)
    {
        try
        {
            await _strategyFileRepository.DeleteAsync(id);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Can't delete strategy {id}", id);
            throw new Exception();
        }
    }
}