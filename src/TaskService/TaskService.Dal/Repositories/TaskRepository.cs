using Microsoft.EntityFrameworkCore;
using TaskService.Dal.Data;
using TaskService.Dal.Models;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Dal.Repositories;

internal class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext context;

    public TaskRepository(TaskDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ServerTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks.AsNoTracking().FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    public async Task<ICollection<ServerTask>> GetExpiredTaskAsync(CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks
            .Where(t => t.CreatedAt + t.Ttl < DateTime.UtcNow &&
                        (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
            .ToListAsync(cancellationToken);
    }

    public async Task<ICollection<ServerTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(ServerTask serverTask, CancellationToken cancellationToken = default)
    {
        await this.context.Tasks.AddAsync(serverTask, cancellationToken);

        await this.context.SaveChangesAsync(cancellationToken);

        return serverTask.Id;
    }

    public async Task UpdateAsync(ServerTask serverTask, CancellationToken cancellationToken = default)
    {
        this.context.Tasks.Update(serverTask);

        await this.context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await this.GetByIdAsync(id, cancellationToken);

        if (task != null)
        {
            this.context.Tasks.Remove(task);

            await this.context.SaveChangesAsync(cancellationToken);
        }
    }
}