using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskService.Model.Data;
using TaskStatus = TaskService.Client.Tasks.TaskStatus;

namespace TaskService.Logic.Services;

public class TaskExpirationService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<TaskExpirationService> logger;
    private readonly TimeSpan checkInterval = TimeSpan.FromSeconds(30);

    public TaskExpirationService(
        IServiceProvider serviceProvider,
        ILogger<TaskExpirationService> logger)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Task Expiration Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await this.CheckAndExpireTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error checking expired tasks");
            }

            await Task.Delay(this.checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndExpireTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();

        var now = DateTime.UtcNow;
        var expiredTasks = await dbContext.Tasks
            .Where(t => t.ExpiresAt.HasValue &&
                        t.ExpiresAt.Value < now &&
                        (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
            .ToListAsync(cancellationToken);

        foreach (var task in expiredTasks)
        {
            this.logger.LogWarning("Marking task {TaskId} as expired", task.Id);

            task.Status = TaskStatus.Expired;
            task.CompletedAt = DateTime.UtcNow;
            task.ErrorMessage = "Task expired due to TTL";

            dbContext.Tasks.Update(task);
        }

        if (expiredTasks.Count != 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            this.logger.LogInformation("Marked {Count} tasks as expired", expiredTasks.Count);
        }
    }
}