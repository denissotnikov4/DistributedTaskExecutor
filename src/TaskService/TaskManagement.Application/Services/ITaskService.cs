using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Services;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskDto> RetryTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
}