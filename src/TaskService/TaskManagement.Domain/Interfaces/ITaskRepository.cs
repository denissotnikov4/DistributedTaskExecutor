using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Domain.Interfaces;

public interface ITaskRepository
{
    Task<Task?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetPendingTasksAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Task>> GetExpiredTasksAsync(CancellationToken cancellationToken = default);
    Task<Task> CreateAsync(Task task, CancellationToken cancellationToken = default);
    Task<Task> UpdateAsync(Task task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

