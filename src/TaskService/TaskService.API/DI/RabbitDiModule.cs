using TaskService.Logic.Services.Messaging;

namespace TaskService.Api.DI;

public class RabbitDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITaskMessageQueue>(
            serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<RabbitMqTaskMessageQueue>>();

                return new RabbitMqTaskMessageQueue(
                    configuration["RabbitMQ:HostName"] ?? "localhost",
                    configuration["RabbitMQ:UserName"] ?? "guest",
                    configuration["RabbitMQ:Password"] ?? "guest",
                    logger);
            });
    }
}