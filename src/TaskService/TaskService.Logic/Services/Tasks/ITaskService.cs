using TaskService.Client.Models.Requests;
using TaskService.Client.Models.Tasks;

namespace TaskService.Logic.Services.Tasks;

public interface ITaskService
{
    Task<Guid> CreateTaskAsync(TaskCreateRequest taskCreateRequest, CancellationToken cancellationToken = default);

    Task<ClientTask?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ICollection<ClientTask>> GetAllTasksAsync(CancellationToken cancellationToken = default);

    Task RetryTaskAsync(Guid id, CancellationToken cancellationToken = default);
}