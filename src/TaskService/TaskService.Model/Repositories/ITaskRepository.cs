using TaskService.Model.Data;

namespace TaskService.Model.Repositories;

public interface ITaskRepository
{
    Task<ServerTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ICollection<ServerTask>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ServerTask serverTask, CancellationToken cancellationToken = default);

    Task UpdateAsync(ServerTask serverTask, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}