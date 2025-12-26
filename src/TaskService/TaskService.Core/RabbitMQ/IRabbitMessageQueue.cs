using System.Text.Json;

namespace TaskService.Core.RabbitMQ;

public interface IRabbitMessageQueue<TMessage> : IDisposable
{
    void Publish(TMessage message, JsonSerializerOptions? jsonOptions = null);

    void Consume(Func<TMessage, Task<bool>> handleMessage, JsonSerializerOptions? jsonOptions = null);
}