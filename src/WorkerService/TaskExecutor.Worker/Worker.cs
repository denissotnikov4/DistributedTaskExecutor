using System.Text.Json;
using TaskEntity = TaskManagement.Domain.Entities.Job;
using TaskExecutor.Application.Services;
using TaskExecutor.Infrastructure.Messaging;
using TaskManagement.Infrastructure.Data;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskExecutor.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqConsumer _consumer;
    private readonly string _workerId;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        RabbitMqConsumer consumer)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _consumer = consumer;
        _workerId = Environment.MachineName + "-" + Guid.NewGuid().ToString("N")[..8];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker {WorkerId} started at: {Time}", _workerId, DateTimeOffset.Now);

        _consumer.StartConsuming(async (task, cancellationToken) =>
        {
            await ProcessTaskAsync(task, cancellationToken);
        }, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessTaskAsync(TaskEntity task, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        var taskProcessor = scope.ServiceProvider.GetRequiredService<ITaskProcessor>();

        // Проверяем, не истекла ли задача
        if (task.IsExpired)
        {
            _logger.LogWarning("Task {TaskId} expired before processing", task.Id);
            await MarkTaskAsExpiredAsync(dbContext, task, cancellationToken);
            return;
        }

        // Обновляем статус задачи
        task.Status = TaskStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;
        task.WorkerId = _workerId;
        dbContext.Tasks.Update(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Worker {WorkerId} started processing task {TaskId}", _workerId, task.Id);

        try
        {
            // Создаем CancellationTokenSource с TTL
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter((int)task.Ttl.TotalMilliseconds);

            // Выполняем задачу
            var result = await taskProcessor.ProcessTaskAsync(task, cts.Token);

            // Сохраняем результат
            task.Status = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.Result = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            task.ErrorMessage = null;

            _logger.LogInformation("Worker {WorkerId} completed task {TaskId}", _workerId, task.Id);
        }
        catch (OperationCanceledException)
        {
            // TTL истек
            _logger.LogWarning("Task {TaskId} cancelled due to TTL expiration", task.Id);
            task.Status = TaskStatus.Expired;
            task.ErrorMessage = "Task execution exceeded TTL";
        }
        catch (Exception ex)
        {
            // Ошибка выполнения
            _logger.LogError(ex, "Error executing task {TaskId}", task.Id);
            task.Status = TaskStatus.Failed;
            task.ErrorMessage = ex.Message;
        }
        finally
        {
            task.CompletedAt = DateTime.UtcNow;
            dbContext.Tasks.Update(task);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task MarkTaskAsExpiredAsync(TaskDbContext dbContext, TaskEntity task, CancellationToken cancellationToken)
    {
        task.Status = TaskStatus.Expired;
        task.CompletedAt = DateTime.UtcNow;
        task.ErrorMessage = "Task expired before execution";
        dbContext.Tasks.Update(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}