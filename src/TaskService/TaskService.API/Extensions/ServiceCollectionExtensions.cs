using Microsoft.EntityFrameworkCore;
using TaskService.Logic.Messaging;
using TaskService.Logic.Services;
using TaskService.Logic.Validators;
using TaskService.Model.Data;
using TaskService.Model.Repositories;

namespace TaskService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITaskMessageQueue>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqTaskMessageQueue>>();

            return new RabbitMqTaskMessageQueue(
                configuration["RabbitMQ:HostName"] ?? "localhost",
                configuration["RabbitMQ:UserName"] ?? "guest",
                configuration["RabbitMQ:Password"] ?? "guest",
                logger);
        });

        services.AddHostedService<TaskExpirationService>();

        services.AddSingleton<CreateTaskDtoValidator>();
        services.AddScoped<ITaskService, Logic.Services.TaskService>();

        return services;
    }

    public static IServiceCollection AddModel(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("TaskService.Logic")));

        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}