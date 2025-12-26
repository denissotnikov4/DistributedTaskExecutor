using TaskService.Core.RabbitMQ;

namespace TaskService.Api.DI;

public class RabbitDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRabbitMessageQueue<Guid>>(
            serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<IRabbitMessageQueue<Guid>>>();
                var settings = serviceProvider.GetRequiredService<RabbitSettings>();

                return new RabbitMessageQueue<Guid>(
                    settings,
                    infoMessage => logger.LogInformation(infoMessage),
                    errorMessage => logger.LogError(errorMessage));
            });
    }
}