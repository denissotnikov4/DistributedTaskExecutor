using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DistributedTaskExecutor.Core.RabbitMQ;

public sealed class RabbitMessageQueue<TMessage> : IRabbitMessageQueue<TMessage>
{
    private readonly IModel channel;
    private readonly IConnection connection;

    private readonly RabbitMqSettings settings;
    private readonly JsonSerializerOptions messageSerializationOptions;
    private readonly ILogger<RabbitMessageQueue<TMessage>> logger;

    private bool disposed;

    public RabbitMessageQueue(
        RabbitMqSettings mqSettings,
        JsonSerializerOptions messageSerializationOptions,
        ILogger<RabbitMessageQueue<TMessage>> logger)
    {
        this.settings = mqSettings ?? throw new ArgumentNullException(nameof(mqSettings));
        this.messageSerializationOptions = messageSerializationOptions ?? throw new ArgumentNullException(nameof(messageSerializationOptions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

            this.logger.LogInformation("RabbitMQ connection established. Queue: {queueName}.", this.settings.QueueName);
        }
        catch (Exception exception)
        {
            this.logger.LogError("Failed to establish RabbitMQ connection: {errorMessage}.", exception.Message);

            throw;
        }
    }

    public void Publish(TMessage message)
    {
        ObjectDisposedException.ThrowIf(this.disposed, nameof(RabbitMessageQueue<TMessage>));

        var messageId = Guid.NewGuid().ToString();

        try
        {
            var json = JsonSerializer.Serialize(message, this.messageSerializationOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = this.channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.MessageId = messageId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.ContentType = "application/json";
            properties.Type = nameof(TMessage);

            this.channel.BasicPublish(this.settings.ExchangeName, this.settings.RoutingKey, true, properties, body);

            this.logger.LogInformation(
                "Message \"{messageId}\" successfully published to exchange \"{exchangeName}\".",
                messageId,
                this.settings.ExchangeName);
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                "Error while publishing Message \"{messageId}\" to Exchange \"{exchangeName}\": {errorMessage}.",
                messageId,
                this.settings.ExchangeName,
                exception.Message);

            throw;
        }
    }

    public void Consume(Func<TMessage, Task<bool>> handleMessage)
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

                var message = JsonSerializer.Deserialize<TMessage>(json, this.messageSerializationOptions);

                if (message == null)
                {
                    throw new JsonException("Deserialized message is null.");
                }

                var isSuccess = await handleMessage(message).ConfigureAwait(false);

                if (isSuccess)
                {
                    this.channel.BasicAck(args.DeliveryTag, false);
                    this.logger.LogInformation("Message \"{messageId}\" successfully acked.", messageId);
                }
                else
                {
                    this.channel.BasicNack(args.DeliveryTag, false, true);
                    this.logger.LogInformation("Message \"{messageId}\" successfully nacked.", messageId);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    "Error while consuming Message \"{messageId}\": {errorMessage}.",
                    messageId,
                    exception.Message);

                this.channel.BasicNack(args.DeliveryTag, false, true);
            }
        };

        this.channel.BasicQos(0, 1, false);

        this.channel.BasicConsume(this.settings.QueueName, false, consumer);

        this.logger.LogInformation("Started consuming queue \"{queueName}\".", this.settings.QueueName);
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

            this.logger.LogInformation("RabbitMQ connection successfully disposed.");
        }
        catch (Exception exception)
        {
            this.logger.LogError("Error while disposing RabbitMQ resources: {errorMessage}.", exception.Message);
        }
    }
}