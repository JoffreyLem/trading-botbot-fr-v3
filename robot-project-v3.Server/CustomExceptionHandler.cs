using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using robot_project_v3.Server.BackgroundService;
using robot_project_v3.Server.Dto;
using RobotAppLibrary.Api.Providers.Exceptions;
using RobotAppLibrary.Strategy;

namespace robot_project_v3.Server;

public class CustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        HttpStatusCode statusCode;

        if (exception is ApiProvidersException or StrategyException or CommandException)
            statusCode = HttpStatusCode.BadRequest;
        else
            statusCode = HttpStatusCode.InternalServerError;

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)statusCode;

        var response = new ApiResponseError
        {
            Error = exception.Message
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
   
        });

        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken: cancellationToken);

        return true;
    }

}