using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobotAppLibrary.LLM.Gemini.Configuration;
using RobotAppLibrary.LLM.Gemini.Repositories;
using Serilog;

namespace RobotAppLibrary.LLM;

// ReSharper disable once InconsistentNaming
public class LLMManager : ILLMManager
{
    private IServiceProvider _serviceProvider;

    public LLMManager(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    // ReSharper disable once InconsistentNaming
    public ILLMRepository GetLLM(Model.LLM llm)
    {
        switch (llm)
        {
            case Model.LLM.Gemini:
                return new GeminiRepository(
                    _serviceProvider.GetService<IHttpClientFactory>(), 
                    _serviceProvider.GetService<ILogger<GeminiRepository>>(), 
                    _serviceProvider.GetRequiredService<IOptions<GeminiSettings>>());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(llm), llm, null);
        }
    }
}