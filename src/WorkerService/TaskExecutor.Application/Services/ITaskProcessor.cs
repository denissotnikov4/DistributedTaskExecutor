using TaskEntity = TaskManagement.Domain.Entities.Job;

namespace TaskExecutor.Application.Services;

public interface ITaskProcessor
{
    Task<object?> ProcessTaskAsync(TaskEntity task, CancellationToken cancellationToken = default);
}

