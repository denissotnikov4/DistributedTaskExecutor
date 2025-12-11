using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaskEntity = TaskManagement.Domain.Entities.Job;
using TaskExecutor.Domain.Interfaces;

namespace TaskExecutor.Application.Services;

public class TaskProcessor : ITaskProcessor
{
    private readonly ICodeExecutor _codeExecutor;
    private readonly ILogger<TaskProcessor> _logger;

    public TaskProcessor(ICodeExecutor codeExecutor, ILogger<TaskProcessor> logger)
    {
        _codeExecutor = codeExecutor;
        _logger = logger;
    }

    public async Task<object?> ProcessTaskAsync(TaskEntity task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing task {TaskId}", task.Id);

        // Если есть C# код, выполняем его
        if (!string.IsNullOrWhiteSpace(task.Code))
        {
            _logger.LogInformation("Executing C# code for task {TaskId}", task.Id);
            return await _codeExecutor.ExecuteAsync(task.Code, task.Payload, cancellationToken);
        }

        // Иначе выполняем стандартную обработку payload
        _logger.LogInformation("Executing standard payload processing for task {TaskId}", task.Id);
        return await ProcessStandardPayloadAsync(task.Payload, cancellationToken);
    }

    private async Task<object> ProcessStandardPayloadAsync(string payload, CancellationToken cancellationToken)
    {
        // Симуляция обработки (от 1 до 5 секунд)
        var random = new Random();
        var delay = random.Next(1000, 5000);
        await Task.Delay(delay, cancellationToken);

        // Обработка payload
        var result = new
        {
            ProcessedAt = DateTime.UtcNow,
            OriginalPayload = payload,
            ProcessedData = $"Processed: {payload}"
        };

        return result;
    }
}

