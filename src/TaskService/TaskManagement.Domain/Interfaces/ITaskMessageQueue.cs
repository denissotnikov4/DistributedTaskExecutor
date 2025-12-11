using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Domain.Interfaces;

public interface ITaskMessageQueue
{
    System.Threading.Tasks.Task PublishTaskAsync(Task task, CancellationToken cancellationToken = default);
    Task<Task?> ConsumeTaskAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task PublishTaskResultAsync(Task task, CancellationToken cancellationToken = default);
}

