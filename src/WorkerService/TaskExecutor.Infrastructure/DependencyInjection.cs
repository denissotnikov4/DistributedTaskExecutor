using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskExecutor.Application.Services;
using TaskExecutor.Domain.Interfaces;
using TaskExecutor.Infrastructure.CodeExecution;
using TaskExecutor.Infrastructure.Messaging;
using TaskManagement.Infrastructure.Data;

namespace TaskExecutor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<TaskDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        // Code Execution
        services.AddScoped<ICodeExecutor, RoslynCodeExecutor>();

        // Messaging
        services.AddSingleton<RabbitMqConsumer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer>>();
            return new RabbitMqConsumer(
                configuration["RabbitMQ:HostName"] ?? "localhost",
                configuration["RabbitMQ:UserName"] ?? "guest",
                configuration["RabbitMQ:Password"] ?? "guest",
                logger);
        });

        // Application Services
        services.AddScoped<ITaskProcessor, TaskProcessor>();

        return services;
    }
}

