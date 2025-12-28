namespace DistributedTaskExecutor.Core.RabbitMQ;

public interface IRabbitMessageQueue<TMessage> : IDisposable
{
    void Publish(TMessage message);

    void Consume(Func<TMessage, Task<bool>> handleMessage);
}