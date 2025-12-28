namespace DistributedTaskExecutor.Core.RabbitMQ;

public class RabbitSettings
{
    public string AppName { get; set; }

    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; }

    public string QueueName { get; set; }
    public string ExchangeName { get; set; }
    public string ExchangeType { get; set; }
    public string RoutingKey { get; set; }
}