using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskService.Dal.Repositories;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Logic.Services.Tasks;

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
        this.logger.LogInformation("Task Expiration Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await this.CheckAndExpireTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error checking expired tasks.");
            }

            await Task.Delay(this.checkInterval, stoppingToken);
        }
    }
    
    private async Task CheckAndExpireTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();
        var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

        var expiredTasks = await taskRepository.GetExpiredTaskAsync(cancellationToken);
        
        foreach (var task in expiredTasks)
        {
            this.logger.LogWarning("Marking task {TaskId} as expired", task.Id);
        
            task.Status = TaskStatus.Expired;
            task.CompletedAt = DateTime.UtcNow;
            task.ErrorMessage = "Task expired due to TTL";
        
            await taskRepository.UpdateAsync(task, cancellationToken);
        }
        
        if (expiredTasks.Count != 0)
        {
            this.logger.LogInformation("Marked {Count} tasks as expired.", expiredTasks.Count);
        }
    }
}