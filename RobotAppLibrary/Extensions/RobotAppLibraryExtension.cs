using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RobotAppLibrary.LLM.Gemini.Extensions;

namespace RobotAppLibrary.Extensions;

public static class RobotAppLibraryExtension
{
    public static void AddRobotAppLibrary(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGeminiHttpClient(configuration);
    }



}