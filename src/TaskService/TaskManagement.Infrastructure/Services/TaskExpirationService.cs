using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Services;

public class TaskExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskExpirationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public TaskExpirationService(
        IServiceProvider serviceProvider,
        ILogger<TaskExpirationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task Expiration Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expired tasks");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndExpireTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();

        var now = DateTime.UtcNow;
        var expiredTasks = await dbContext.Tasks
            .Where(t => t.ExpiresAt.HasValue && 
                       t.ExpiresAt.Value < now && 
                       (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
            .ToListAsync(cancellationToken);

        foreach (var task in expiredTasks)
        {
            _logger.LogWarning("Marking task {TaskId} as expired", task.Id);
            
            task.Status = TaskStatus.Expired;
            task.CompletedAt = DateTime.UtcNow;
            task.ErrorMessage = "Task expired due to TTL";
            
            dbContext.Tasks.Update(task);
        }

        if (expiredTasks.Any())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Marked {Count} tasks as expired", expiredTasks.Count);
        }
    }
}

