using System.Threading.Channels;
using Destructurama;
using robot_project_v3.Database;
using robot_project_v3.Mail;
using robot_project_v3.Server.BackgroundService;
using robot_project_v3.Server.Command.Api;
using robot_project_v3.Server.Command.Strategy;
using robot_project_v3.Server.Mapper;
using robot_project_v3.Server.Services;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;

namespace robot_project_v3.Server;

public static class ProgramConfigurationHelper
{
    public static void AddLogger(this WebApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithExceptionDetails()
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ApplicationName", "Robot-API")
            .Enrich.With(new RemovePropertiesEnricher())
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .WriteTo.Async(writeTo => writeTo.Console());

        if (builder.Environment.IsDevelopment())
        {
            loggerConfig.WriteTo.Async(writeTo => writeTo.Seq(builder.Configuration["Seq:Url"]));
        }

        if (builder.Environment.IsProduction())
        {
            loggerConfig.WriteTo.Async(writeTo =>
                writeTo.Seq(
                    builder.Configuration["Seq:Url"], 
                    apiKey: builder.Configuration["Seq:Apikey"]));
        }


        var logger = loggerConfig.CreateLogger();
        Log.Logger = logger;
        SelfLog.Enable(Console.Error);
        builder.Host.UseSerilog(logger);
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                builder => builder.WithOrigins("https://localhost:5173", "https://robot.botbot.fr")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
    }

    public static void AddBotDependency(this WebApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfilesBackgroundServices>(); },
            typeof(MappingProfilesBackgroundServices).Assembly
        );
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(nameof(SmtpSettings)));
        builder.AddLogger();
        builder.Services.AddSignalR();
        builder.Services.AddHostedService<BotBackgroundService>();
        builder.Services.AddStrategyDbContext(builder.Configuration);
        builder.Services.AddSingleton<IApiProviderService, ApiProviderService>();
        builder.Services.AddSingleton<IStrategyService, StrategyService>();
      //  builder.Services.AddSingleton<IStrategyGeneratorService, StrategyGeneratorService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddSingleton<ICommandHandler, CommandHandler>();
        var channelApi = Channel.CreateUnbounded<CommandeBaseApiAbstract>();
        builder.Services.AddSingleton(channelApi.Reader);
        builder.Services.AddSingleton(channelApi.Writer);
        var channelStrategy =
            Channel.CreateUnbounded<CommandeBaseStrategyAbstract>();
        builder.Services.AddSingleton(channelStrategy.Reader);
        builder.Services.AddSingleton(channelStrategy.Writer);

   

        builder.Services.AddSignalR();
    }
}