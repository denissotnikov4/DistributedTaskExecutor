using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;

namespace TaskService.Logic.Services.Tasks;

public interface ITaskService
{
    Task<Guid> CreateTaskAsync(TaskCreateRequest taskCreateRequest, CancellationToken cancellationToken = default);

    Task<ClientTask?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ICollection<ClientTask>> GetAllTasksAsync(CancellationToken cancellationToken = default);

    Task RetryTaskAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task UpdateTaskAsync(Guid id, TaskUpdateRequest taskUpdateRequest, CancellationToken cancellationToken = default);
}