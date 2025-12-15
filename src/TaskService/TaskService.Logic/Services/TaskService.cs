using AutoMapper;
using TaskService.Client.Requests;
using TaskService.Logic.Messaging;
using TaskService.Model.Data;
using TaskService.Model.Repositories;
using TaskStatus = TaskService.Client.Tasks.TaskStatus;

namespace TaskService.Logic.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository taskRepository;
    private readonly ITaskMessageQueue messageQueue;
    private readonly IMapper mapper;

    public TaskService(
        ITaskRepository taskRepository,
        ITaskMessageQueue messageQueue,
        IMapper mapper)
    {
        this.taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        this.messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Guid> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var serverTask = this.mapper.Map<ServerTask>(request);

        var createdId = await this.taskRepository.CreateAsync(serverTask, cancellationToken);

        // await this.messageQueue.PublishTaskAsync(createdId, cancellationToken);

        return createdId;
    }

    public async Task<ServerTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);

        return task == null ? null : this.mapper.Map<ServerTask>(task);
    }

    public async Task<ICollection<ServerTask>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await this.taskRepository.GetAllAsync(cancellationToken);

        return this.mapper.Map<ICollection<ServerTask>>(tasks);
    }

    public async Task RetryTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(id, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with id {id} not found.");
        }

        if (task.Status != TaskStatus.Failed && task.Status != TaskStatus.Expired)
        {
            throw new InvalidOperationException($"Task {id} cannot be retried. Current status: {task.Status}.");
        }

        if (task.RetryCount >= task.MaxRetries)
        {
            throw new InvalidOperationException($"Task {id} has exceeded max retries ({task.MaxRetries}).");
        }

        task.Status = TaskStatus.Pending;
        task.StartedAt = null;
        task.CompletedAt = null;
        task.ErrorMessage = null;
        task.WorkerId = null;
        task.RetryCount++;
        task.ExpiresAt = DateTime.UtcNow.Add(task.Ttl);

        await this.taskRepository.UpdateAsync(task, cancellationToken);

        // await this.messageQueue.PublishTaskAsync(updatedTask, cancellationToken);
    }
}