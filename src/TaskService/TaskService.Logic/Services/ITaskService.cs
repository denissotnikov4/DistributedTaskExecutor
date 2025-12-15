using TaskService.Client.Requests;
using TaskService.Model.Data;

namespace TaskService.Logic.Services;

public interface ITaskService
{
    Task<Guid> CreateTaskAsync(CreateTaskRequest createRequest, CancellationToken cancellationToken = default);

    Task<ServerTask?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ICollection<ServerTask>> GetAllTasksAsync(CancellationToken cancellationToken = default);

    Task RetryTaskAsync(Guid id, CancellationToken cancellationToken = default);
}