using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TaskService.Core.RabbitMQ;

public sealed class RabbitMessageQueue<TMessage> : IRabbitMessageQueue<TMessage>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IModel channel;

    private readonly IConnection connection;
    private readonly Action<string> logError;
    private readonly Action<string> logInfo;

    private readonly RabbitSettings settings;

    private bool disposed;

    public RabbitMessageQueue(
        RabbitSettings settings,
        Action<string> logInfo,
        Action<string> logError)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        this.logInfo = logInfo ?? throw new ArgumentNullException(nameof(logInfo));
        this.logError = logError ?? throw new ArgumentNullException(nameof(logError));

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = this.settings.HostName,
                UserName = this.settings.UserName,
                Password = this.settings.Password,
                Port = this.settings.Port,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            this.connection = factory.CreateConnection($"{this.settings.AppName}_connection");

            this.channel = this.connection.CreateModel();

            this.channel.ExchangeDeclare(this.settings.ExchangeName, this.settings.ExchangeType, true, false, null);

            this.channel.QueueDeclare(this.settings.QueueName, true, false, false);

            this.channel.QueueBind(this.settings.QueueName, this.settings.ExchangeName, this.settings.RoutingKey);

            this.logInfo($"RabbitMQ connection established. Queue: {this.settings.QueueName}.");
        }
        catch (Exception exception)
        {
            this.logError($"Failed to establish RabbitMQ connection, Reason: {exception.Message}.");
            throw;
        }
    }

    public void Publish(TMessage message, JsonSerializerOptions? jsonOptions = null)
    {
        ObjectDisposedException.ThrowIf(this.disposed, nameof(RabbitMessageQueue<TMessage>));

        var messageId = Guid.NewGuid().ToString();

        try
        {
            var json = JsonSerializer.Serialize(message, jsonOptions ?? DefaultJsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = this.channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.MessageId = messageId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.ContentType = "application/json";
            properties.Type = nameof(TMessage);

            this.channel.BasicPublish(this.settings.ExchangeName, this.settings.RoutingKey, true, properties, body);

            this.logInfo($"Message \"{messageId}\" successfully published to exchange \"{this.settings.ExchangeName}\".");
        }
        catch (Exception exception)
        {
            this.logError($"Error while publishing Message \"{messageId}\" to Exchange \"{this.settings.ExchangeName}\": {exception.Message}.");
            throw;
        }
    }

    public void Consume(Func<TMessage, Task<bool>> handleMessage, JsonSerializerOptions? jsonOptions = null)
    {
        ObjectDisposedException.ThrowIf(this.disposed, nameof(RabbitMessageQueue<TMessage>));

        var consumer = new AsyncEventingBasicConsumer(this.channel);

        consumer.Received += async (_, args) =>
        {
            var messageId = args.BasicProperties?.MessageId ?? "unknown";

            try
            {
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var message = JsonSerializer.Deserialize<TMessage>(json, jsonOptions ?? DefaultJsonOptions);

                if (message == null)
                {
                    throw new JsonException("Deserialized message is null.");
                }

                var isSuccess = await handleMessage(message).ConfigureAwait(false);

                if (isSuccess)
                {
                    this.channel.BasicAck(args.DeliveryTag, false);
                    this.logInfo($"Message \"{messageId}\" successfully acked.");
                }
                else
                {
                    this.channel.BasicNack(args.DeliveryTag, false, true);
                    this.logInfo($"Message \"{messageId}\" successfully nacked.");
                }
            }
            catch (Exception exception)
            {
                this.logError($"Error while consuming Message \"{messageId}\": {exception.Message}.");
                this.channel.BasicNack(args.DeliveryTag, false, true);
            }
        };

        this.channel.BasicQos(0, 1, false);

        this.channel.BasicConsume(this.settings.QueueName, false, consumer);

        this.logInfo($"Started consuming queue \"{this.settings.QueueName}\".");
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        try
        {
            if (this.channel is { IsOpen: true })
            {
                this.channel.Close();
                this.channel.Dispose();
            }

            if (this.connection is { IsOpen: true })
            {
                this.connection.Close();
                this.connection.Dispose();
            }

            this.logInfo("RabbitMQ connection successfully disposed.");
        }
        catch (Exception exception)
        {
            this.logError($"Error while disposing RabbitMQ resources: {exception.Message}.");
        }
    }
}