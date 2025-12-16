using TaskService.Client.Models.Tasks;

namespace TaskService.Logic.Messaging;

public interface ITaskMessageQueue
{
    Task PublishTaskAsync(ClientTask task, CancellationToken cancellationToken = default);

    Task<ClientTask?> ConsumeTaskAsync(CancellationToken cancellationToken = default);

    Task PublishTaskResultAsync(ClientTask task, CancellationToken cancellationToken = default);
}