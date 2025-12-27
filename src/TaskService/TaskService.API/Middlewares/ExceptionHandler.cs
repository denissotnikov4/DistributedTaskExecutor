using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskService.Logic.Exceptions.Base;

namespace TaskService.Api.Middlewares;

internal class ExceptionHandler
{
    private readonly RequestDelegate next;
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<ExceptionHandler> logger;

    public ExceptionHandler(RequestDelegate next, IWebHostEnvironment environment, ILogger<ExceptionHandler> logger)
    {
        this.next = next;
        this.environment = environment;
        this.logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        this.logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(context, exception);
        
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = this.environment.IsDevelopment()
        };

        await context.Response.WriteAsJsonAsync(problemDetails, jsonOptions);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        return exception switch
        {
            TaskServiceException taskServiceException => CreateTaskServiceProblemDetails(
                taskServiceException, traceId),
            
            _ => CreateDefaultProblemDetails(exception, traceId)
        };
    }

    private ProblemDetails CreateTaskServiceProblemDetails(
        TaskServiceException exception, 
        string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{exception.StatusCode}",
            Title = exception.GetType().Name.Replace("Exception", ""),
            Status = exception.StatusCode,
            Detail = exception.Message,
            Extensions =
            {
                ["traceId"] = traceId,
                ["errorCode"] = exception.ErrorCode
            }
        };

        // Добавляем дополнительные поля из ToProblemDetails()
        if (exception.ToProblemDetails() is var customDetails)
        {
            var json = JsonSerializer.Serialize(customDetails);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            foreach (var kvp in dict!)
            {
                if (!problemDetails.Extensions.ContainsKey(kvp.Key))
                {
                    problemDetails.Extensions[kvp.Key] = kvp.Value;
                }
            }
        }

        return problemDetails;
    }

    private ProblemDetails CreateDefaultProblemDetails(Exception exception, string traceId)
    {
        return new ProblemDetails
        {
            Type = "https://httpstatuses.com/500",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = this.environment.IsDevelopment() 
                ? exception.ToString() 
                : "An unexpected error occurred",
            Extensions =
            {
                ["traceId"] = traceId,
                ["errorCode"] = "INTERNAL_SERVER_ERROR"
            }
        };
    }
}