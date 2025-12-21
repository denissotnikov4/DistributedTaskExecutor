using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Core.Configuration;

namespace Core.Messaging;

public class RabbitMqConnectionFactory(RabbitMqOptions options, ILogger<RabbitMqConnectionFactory> logger)
{
    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            Port = options.Port
        };

        try
        {
            var connection = factory.CreateConnection();
            logger.LogInformation("RabbitMQ connection established to {HostName}:{Port}",
                options.HostName, options.Port);
            return connection;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create RabbitMQ connection to {HostName}:{Port}",
                options.HostName, options.Port);
            throw;
        }
    }

    public static RabbitMqOptions GetOptions(IConfiguration configuration)
    {
        var options = new RabbitMqOptions();
        configuration.GetSection(RabbitMqOptions.SectionName).Bind(options);
        return options;
    }
}