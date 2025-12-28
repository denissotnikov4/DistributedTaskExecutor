using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DistributedTaskExecutor.Core.DI;
using DistributedTaskExecutor.Core.RabbitMQ;

namespace TaskService.Api.DI;

public class RabbitDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRabbitMessageQueue<Guid>>(
            serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<RabbitMqSettings>();

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var logger = serviceProvider.GetRequiredService<ILogger<RabbitMessageQueue<Guid>>>();

                return new RabbitMessageQueue<Guid>(settings, jsonOptions, logger);
            });
    }
}