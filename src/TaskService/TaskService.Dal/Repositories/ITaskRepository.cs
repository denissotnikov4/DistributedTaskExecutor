using TaskService.Dal.Models;

namespace TaskService.Dal.Repositories;

public interface ITaskRepository
{
    Task<ServerTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ICollection<ServerTask>> GetExpiredTaskAsync(CancellationToken cancellationToken = default);

    Task<ICollection<ServerTask>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ServerTask serverTask, CancellationToken cancellationToken = default);

    Task UpdateAsync(ServerTask serverTask, CancellationToken cancellationToken = default);

    // В контроллере должен быть соответствующий метод с валидацией возможности удаления.
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}