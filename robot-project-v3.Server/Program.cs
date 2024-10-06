using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Destructurama;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using robot_project_v3.Database;
using robot_project_v3.Database.DbContext;
using robot_project_v3.Mail;
using robot_project_v3.Server;
using robot_project_v3.Server.BackgroundService;
using robot_project_v3.Server.BackgroundService.Command.Api;
using robot_project_v3.Server.BackgroundService.Command.Strategy;
using robot_project_v3.Server.Hubs;
using robot_project_v3.Server.Mapper;
using robot_project_v3.Server.Services;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Seq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();
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
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithExceptionDetails()
    .Enrich.With<CustomExceptionEnricher>()
    .Destructure.With<ApiProvidersExceptionDestructuringPolicy>()
    .Destructure.UsingAttributes()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("ApplicationName", "Robot-API")
    .Enrich.WithCorrelationIdHeader("correlationId")
    .Enrich.With(new RemovePropertiesEnricher())
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .WriteTo.Async(writeTo => writeTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{@Exception}{@Properties}{NewLine}"
    ));

if (builder.Environment.IsDevelopment())
    loggerConfig.WriteTo.Async(writeTo => writeTo.Seq(builder.Configuration["Seq:Url"]));

if (builder.Environment.IsProduction())
    loggerConfig.WriteTo.Async(writeTo =>
        writeTo.Seq(
            builder.Configuration["Seq:Url"],
            apiKey: builder.Configuration["Seq:Apikey"]));


var logger = loggerConfig.CreateLogger();
Log.Logger = logger;
SelfLog.Enable(Console.Error);
builder.Host.UseSerilog(logger);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfilesBackgroundServices>(); },
    typeof(MappingProfilesBackgroundServices).Assembly
);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(nameof(SmtpSettings)));
builder.Services.AddSignalR();
builder.Services.AddHostedService<BotBackgroundService>();
builder.Services.AddStrategyDbContext(builder.Configuration);
builder.Services.AddSingleton<IApiProviderService, ApiProviderService>();
builder.Services.AddSingleton<IStrategyService, StrategyService>();
builder.Services.AddSingleton<IStrategyBuilderService, StrategyBuilderService>();
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

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StrategyContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors("AllowSpecificOrigin");
app.MapHub<HubInfoClient>("/infoClient").RequireCors("AllowSpecificOrigin");
app.MapFallbackToFile("/index.html");

app.Run();