using TaskService.Client.Models.Tasks;

namespace TaskService.Logic.Services.Messaging;

public interface ITaskMessageQueue
{
    public Task PublishTaskAsync(ClientTask task, CancellationToken cancellationToken = default);

    Task<ClientTask?> ConsumeTaskAsync(CancellationToken cancellationToken = default);
}