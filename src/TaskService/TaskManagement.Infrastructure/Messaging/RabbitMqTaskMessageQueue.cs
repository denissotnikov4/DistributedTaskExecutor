using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Infrastructure.Messaging;

public class RabbitMqTaskMessageQueue : ITaskMessageQueue, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqTaskMessageQueue> _logger;
    private const string TaskQueueName = "task_queue";
    private const string ResultQueueName = "task_result_queue";
    private const string ExchangeName = "task_exchange";

    public RabbitMqTaskMessageQueue(
        string hostName,
        string userName,
        string password,
        ILogger<RabbitMqTaskMessageQueue> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = 5672
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Объявляем exchange и очереди
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
        _channel.QueueDeclare(TaskQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare(ResultQueueName, durable: true, exclusive: false, autoDelete: false);
        
        _channel.QueueBind(TaskQueueName, ExchangeName, "task");
        _channel.QueueBind(ResultQueueName, ExchangeName, "result");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public System.Threading.Tasks.Task PublishTaskAsync(Task task, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Serialize(task, JsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = task.Id.ToString();
            properties.Expiration = ((long)task.Ttl.TotalMilliseconds).ToString();

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: "task",
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Task {TaskId} published to queue", task.Id);
            return System.Threading.Tasks.Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing task {TaskId} to queue", task.Id);
            throw;
        }
    }

    public System.Threading.Tasks.Task<Task?> ConsumeTaskAsync(CancellationToken cancellationToken = default)
    {
        // Этот метод используется для синхронного потребления
        // В реальном приложении воркеры используют свой собственный механизм потребления
        // Этот интерфейс оставлен для совместимости, но фактическое потребление происходит в Worker
        throw new NotSupportedException("Use Worker service for consuming tasks from queue");
    }

    public System.Threading.Tasks.Task PublishTaskResultAsync(Task task, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Serialize(task, JsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = task.Id.ToString();

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: "result",
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Task result {TaskId} published to result queue", task.Id);
            return System.Threading.Tasks.Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing task result {TaskId} to queue", task.Id);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

