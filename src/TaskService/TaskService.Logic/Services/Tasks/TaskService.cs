using AutoMapper;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;
using TaskService.Core.RabbitMQ;
using TaskService.Dal.Repositories;
using TaskService.Logic.Exceptions;
using TaskService.Logic.Mappings;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Logic.Services.Tasks;

internal class TaskService : ITaskService
{
    private readonly ITaskRepository taskRepository;
    private readonly IRabbitMessageQueue<Guid> messageQueue;

    public TaskService(IRabbitMessageQueue<Guid> messageQueue, ITaskRepository taskRepository)
    {
        this.taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        this.messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
    }

    public async Task<Guid> CreateTaskAsync(TaskCreateRequest request, CancellationToken cancellationToken = default)
    {
        var taskId = await this.taskRepository.CreateAsync(request.ToServerModel(), cancellationToken);

        this.messageQueue.Publish(taskId);

        return taskId;
    }

    public async Task<ClientTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return (await this.taskRepository.GetByIdAsync(taskId, cancellationToken))?.ToClientModel();
    }

    public async Task<ICollection<ClientTask>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        var serverTasks = await this.taskRepository.GetAllAsync(cancellationToken);

        return serverTasks.Select(task => task.ToClientModel()).ToArray();
    }

    public async Task RetryTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var serverTask = await this.taskRepository.GetByIdAsync(id, cancellationToken);

        if (serverTask == null)
        {
            throw new TaskNotFoundException(id);
        }

        if (serverTask.Status is TaskStatus.Pending or TaskStatus.InProgress)
        {
            throw new TaskCannotBeRetriedException(id, serverTask.Status);
        }

        serverTask.Result = null;
        serverTask.Status = TaskStatus.Pending;
        serverTask.StartedAt = null;
        serverTask.CompletedAt = null;
        serverTask.ErrorMessage = null;
        serverTask.RetryCount += 1;

        await this.taskRepository.UpdateAsync(serverTask, cancellationToken);

        this.messageQueue.Publish(id);
    }

    public async Task UpdateTaskAsync(
        Guid id, TaskUpdateRequest taskUpdateRequest, CancellationToken cancellationToken = default)
    {
        var existingTask = await this.taskRepository.GetByIdAsync(id, cancellationToken);

        if (existingTask == null)
        {
            throw new TaskNotFoundException(id);
        }

        var serverTask = existingTask.UpdateServerTaskFromRequest(taskUpdateRequest);

        await this.taskRepository.UpdateAsync(serverTask, cancellationToken);
    }
}