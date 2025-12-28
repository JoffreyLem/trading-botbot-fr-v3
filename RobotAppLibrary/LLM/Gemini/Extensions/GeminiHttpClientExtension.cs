using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RobotAppLibrary.LLM.Gemini.Configuration;
using RobotAppLibrary.LLM.Gemini.Handlers;
using RobotAppLibrary.LLM.Gemini.Repositories;
using RobotAppLibrary.LLM.Interfaces;

namespace RobotAppLibrary.LLM.Gemini.Extensions;

public static class GeminiHttpClientExtension
{
    internal static void AddGeminiHttpClient(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddHttpClient();
        services.Configure<GeminiSettings>(configuration.GetSection("Gemini"));
        services.AddTransient<HttpGeminiLogger>();
        services.AddHttpClient("Gemini", (serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<GeminiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
            })
            .AddLogger<HttpGeminiLogger>();

        services.AddScoped<ILLMRepository, GeminiRepository>();
    }
}