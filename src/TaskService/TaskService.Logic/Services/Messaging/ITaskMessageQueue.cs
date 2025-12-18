using TaskService.Client.Models.Tasks;

namespace TaskService.Logic.Services.Messaging;

public interface ITaskMessageQueue
{
    Task PublishTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<ClientTask?> ConsumeTaskAsync(CancellationToken cancellationToken = default);
}