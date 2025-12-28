using DistributedTaskExecutor.Core.RabbitMQ;
using Vostok.Configuration.Abstractions.Attributes;

namespace WorkerService.Cli.Settings;

public class RabbitSettings
{
    public string AppName { get; init; } = "WorkerService";

    [Required]
    public string HostName { get; init; }

    [Required]
    public string UserName { get; init; }

    [Required]
    public string Password { get; init; }

    [Required]
    public int? Port { get; init; }

    [Required]
    public string QueueName { get; init; }

    [Required]
    public string ExchangeName { get; init; }

    [Required]
    public string ExchangeType { get; init; }

    [Required]
    public string RoutingKey { get; init; }

    public RabbitMqSettings ToCoreSettings()
    {
        return new RabbitMqSettings
        {
            AppName = this.AppName,
            HostName = this.HostName,
            UserName = this.UserName,
            Password = this.Password,
            Port = this.Port!.Value,
            QueueName = this.QueueName,
            ExchangeName = this.ExchangeName,
            ExchangeType = this.ExchangeType,
            RoutingKey = this.RoutingKey
        };
    }
}