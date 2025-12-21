using Microsoft.EntityFrameworkCore;
using TaskService.Dal.Data;
using TaskService.Dal.Models;

namespace TaskService.Dal.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext context;

    public TaskRepository(TaskDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ServerTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks.FindAsync([id], cancellationToken);
    }

    public async Task<ICollection<ServerTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks.ToListAsync(cancellationToken);
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