using TaskService.Core.RabbitMQ;

namespace TaskService.Tests.RabbitMQ;

/// <summary>
/// Требуется запущенный RabbitMQ на хосте.
/// </summary>
[TestFixture]
public class RabbitMessageQueueTests
{
    [Test]
    public async Task PublishAndConsume_Success()
    {
        // Arrange
        var publishedTaskId = Guid.NewGuid();

        using var rabbitMessageQueue = GetMessageQueue<Guid>();

        var tcs = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Act
        rabbitMessageQueue.Consume(id =>
        {
            tcs.TrySetResult(id);
            return Task.FromResult(true);
        });

        rabbitMessageQueue.Publish(publishedTaskId);

        // Assert
        try
        {
            var consumeId = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(15));

            Assert.That(consumeId, Is.EqualTo(publishedTaskId));
        }
        catch (TimeoutException)
        {
            Assert.Fail("Test timed out.");
        }
    }

    private static IRabbitMessageQueue<TModel> GetMessageQueue<TModel>()
    {
        var queueName = $"queue_{Guid.NewGuid()}";
        var exchangeName = $"{queueName}_exchange";

        var settings = new RabbitSettings
        {
            AppName = "TaskService.Tests.Rabbit",
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672,
            QueueName = queueName,
            ExchangeName = exchangeName,
            ExchangeType = "direct",
            RoutingKey = "task-id"
        };

        return new RabbitMessageQueue<TModel>(settings, _ => { }, _ => { });
    }
}