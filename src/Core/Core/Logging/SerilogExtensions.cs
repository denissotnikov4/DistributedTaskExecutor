using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Core.Logging;

public static class SerilogExtensions
{
    public static IHostBuilder UseCoreSerilog(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? logFilePath = null)
    {
        Log.Logger = new LoggerConfiguration()
            .ConfigureSerilog(configuration, logFilePath)
            .CreateLogger();

        return hostBuilder.UseSerilog();
    }

    private static LoggerConfiguration ConfigureSerilog(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string? logFilePath = null)
    {
        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        if (!string.IsNullOrEmpty(logFilePath))
        {
            loggerConfiguration.WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day);
        }

        return loggerConfiguration;
    }
}