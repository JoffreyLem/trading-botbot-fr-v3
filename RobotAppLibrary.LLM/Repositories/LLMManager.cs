using Microsoft.Extensions.Logging;
using RobotAppLibrary.LLM.Repositories.Gemini.Repositories;
using RobotAppLibrary.LLM.Repositories.Models;

namespace RobotAppLibrary.LLM.Repositories;

public class LLMManager
{
    private readonly IServiceProvider _serviceProvider;

    public LLMManager(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public ILLMModel GetLLMModel(Model model)
    {
        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        switch (model)
        {
            case Model.Gemini:
                return new GeminiRepository(, _serviceProvider.GetService<ILogger<GeminiRepository>())
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(model), model, null);
        }
    }
}