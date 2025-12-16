using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TaskService.Client.Models.Tasks;

namespace TaskService.Logic.Messaging;

public class RabbitMqTaskMessageQueue : ITaskMessageQueue, IDisposable
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly ILogger<RabbitMqTaskMessageQueue> logger;

    private const string TaskQueueName = "task_queue";
    private const string ResultQueueName = "task_result_queue";
    private const string ExchangeName = "task_exchange";

    public RabbitMqTaskMessageQueue(
        string hostName,
        string userName,
        string password,
        ILogger<RabbitMqTaskMessageQueue> logger)
    {
        this.logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = 5672
        };

        this.connection = factory.CreateConnection();
        this.channel = this.connection.CreateModel();

        // Объявляем exchange и очереди
        this.channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true);
        this.channel.QueueDeclare(TaskQueueName, true, false, false);
        this.channel.QueueDeclare(ResultQueueName, true, false, false);

        this.channel.QueueBind(TaskQueueName, ExchangeName, "task");
        this.channel.QueueBind(ResultQueueName, ExchangeName, "result");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task PublishTaskAsync(ClientTask task, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Serialize(task, JsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = this.channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = task.Id.ToString();
            properties.Expiration = ((long)task.Ttl.TotalMilliseconds).ToString();

            this.channel.BasicPublish(
                ExchangeName,
                "task",
                properties,
                body);

            this.logger.LogInformation("Task {TaskId} published to queue", task.Id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error publishing task {TaskId} to queue", task.Id);
            throw;
        }
    }

    public Task<ClientTask?> ConsumeTaskAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use Worker service for consuming tasks from queue");
    }

    public Task PublishTaskResultAsync(ClientTask task, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Serialize(task, JsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = this.channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = task.Id.ToString();

            this.channel.BasicPublish(
                ExchangeName,
                "result",
                properties,
                body);

            this.logger.LogInformation("Task result {TaskId} published to result queue", task.Id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error publishing task result {TaskId} to queue", task.Id);
            throw;
        }
    }

    public void Dispose()
    {
        this.channel?.Close();
        this.connection?.Close();
        this.channel?.Dispose();
        this.connection?.Dispose();
    }
}