using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Messaging;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<TaskDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("TaskManagement.Infrastructure")));

        // Repositories
        services.AddScoped<ITaskRepository, TaskRepository>();

        // RabbitMQ
        services.AddSingleton<ITaskMessageQueue>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RabbitMqTaskMessageQueue>>();
            return new RabbitMqTaskMessageQueue(
                configuration["RabbitMQ:HostName"] ?? "localhost",
                configuration["RabbitMQ:UserName"] ?? "guest",
                configuration["RabbitMQ:Password"] ?? "guest",
                logger);
        });

        // Background Services
        services.AddHostedService<TaskExpirationService>();

        return services;
    }
}

