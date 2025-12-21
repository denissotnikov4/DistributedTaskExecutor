using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Core.Configuration;
using RabbitMQ.Client;

namespace Core.Messaging;

public static class RabbitMqDiExtensions
{
    public static IServiceCollection AddRabbitMqOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = RabbitMqConnectionFactory.GetOptions(configuration);
        services.AddSingleton(options);
        return services;
    }

    public static IServiceCollection AddRabbitMqConnectionFactory(
        this IServiceCollection services)
    {
        services.AddSingleton<RabbitMqConnectionFactory>(sp =>
        {
            var options = sp.GetRequiredService<RabbitMqOptions>();
            var logger = sp.GetRequiredService<ILogger<RabbitMqConnectionFactory>>();
            return new RabbitMqConnectionFactory(options, logger);
        });

        return services;
    }

    public static IServiceCollection AddRabbitMqConnection(
        this IServiceCollection services)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<RabbitMqConnectionFactory>();
            return factory.CreateConnection();
        });

        return services;
    }
}