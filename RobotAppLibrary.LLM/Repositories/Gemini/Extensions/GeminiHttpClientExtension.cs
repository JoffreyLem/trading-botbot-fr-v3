using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RobotAppLibrary.LLM.Repositories.Gemini.Configuration;
using RobotAppLibrary.LLM.Repositories.Gemini.Repositories;

namespace RobotAppLibrary.LLM.Repositories.Gemini.Extensions;

internal static class GeminiHttpClientExtension
{
    public static void AddGeminiHttpClient(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<GeminiSettings>(configuration.GetSection("Gemini"));
        services.AddHttpClient("Gemini", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<GeminiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        services.AddScoped<GeminiRepository>();
    }
}