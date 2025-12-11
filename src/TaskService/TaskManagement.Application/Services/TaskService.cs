using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskMessageQueue _messageQueue;
    private readonly IMapper _mapper;

    public TaskService(
        ITaskRepository taskRepository,
        ITaskMessageQueue messageQueue,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _messageQueue = messageQueue;
        _mapper = mapper;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var task = new Task
        {
            Id = Guid.NewGuid(),
            Description = dto.Description,
            Payload = dto.Payload,
            Code = dto.Code,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Ttl = dto.Ttl,
            ExpiresAt = DateTime.UtcNow.Add(dto.Ttl),
            MaxRetries = dto.MaxRetries,
            RetryCount = 0
        };

        var createdTask = await _taskRepository.CreateAsync(task, cancellationToken);
        
        // Отправляем задачу в очередь
        await _messageQueue.PublishTaskAsync(createdTask, cancellationToken);

        return _mapper.Map<TaskDto>(createdTask);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        return task == null ? null : _mapper.Map<TaskDto>(task);
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<TaskDto> RetryTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        
        if (task == null)
            throw new InvalidOperationException($"Task with id {taskId} not found");

        if (task.Status != TaskStatus.Failed && task.Status != TaskStatus.Expired)
            throw new InvalidOperationException($"Task {taskId} cannot be retried. Current status: {task.Status}");

        if (task.RetryCount >= task.MaxRetries)
            throw new InvalidOperationException($"Task {taskId} has exceeded max retries ({task.MaxRetries})");

        // Сбрасываем задачу для повторного выполнения
        task.Status = TaskStatus.Pending;
        task.StartedAt = null;
        task.CompletedAt = null;
        task.ErrorMessage = null;
        task.WorkerId = null;
        task.RetryCount++;
        task.ExpiresAt = DateTime.UtcNow.Add(task.Ttl);

        var updatedTask = await _taskRepository.UpdateAsync(task, cancellationToken);
        
        // Отправляем задачу в очередь для повторного выполнения
        await _messageQueue.PublishTaskAsync(updatedTask, cancellationToken);

        return _mapper.Map<TaskDto>(updatedTask);
    }
}

