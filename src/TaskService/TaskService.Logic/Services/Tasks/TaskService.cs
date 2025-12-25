using TaskService.Client.Models.Requests;
using TaskService.Client.Models.Tasks;
using TaskService.Dal.Repositories;
using TaskService.Logic.Mappings;
using TaskService.Logic.Services.Messaging;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Logic.Services.Tasks;

public class TaskService : ITaskService
{
    private readonly ITaskRepository taskRepository;
    private readonly ITaskMessageQueue messageQueue;

    public TaskService(
        ITaskRepository taskRepository,
        ITaskMessageQueue messageQueue)
    {
        this.taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        this.messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
    }

    public async Task<Guid> CreateTaskAsync(TaskCreateRequest request, CancellationToken cancellationToken = default)
    {
        var serverTask = request.ToServerModel();
        var taskId = await this.taskRepository.CreateAsync(serverTask, cancellationToken);

        var createdTask = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (createdTask != null)
        {
            var clientTask = createdTask.ToClientModel();
            await this.messageQueue.PublishTaskAsync(clientTask, cancellationToken);
        }

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
            throw new InvalidOperationException($"Task with id {id} not found.");
        }

        if (serverTask.Status is TaskStatus.Pending or TaskStatus.InProgress)
        {
            throw new InvalidOperationException($"Task {id} cannot be retried due to current status: {serverTask.Status}.");
        }

        serverTask.Result = null;
        serverTask.Status = TaskStatus.Pending;
        serverTask.StartedAt = null;
        serverTask.CompletedAt = null;
        serverTask.ErrorMessage = null;
        serverTask.RetryCount++;

        await this.taskRepository.UpdateAsync(serverTask, cancellationToken);

        var clientTask = serverTask.ToClientModel();
        await this.messageQueue.PublishTaskAsync(clientTask, cancellationToken);
    }
}