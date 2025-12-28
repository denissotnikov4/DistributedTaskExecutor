using Microsoft.AspNetCore.Http;
using TaskService.Logic.Exceptions.Base;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Logic.Exceptions;

public class TaskCannotBeRetriedException : TaskServiceException
{
    private Guid TaskId { get; }
    private TaskStatus Status { get; }

    public TaskCannotBeRetriedException(Guid taskId, TaskStatus status)
        : base($"Task {taskId} cannot be retried due to current status: {status}.",
            StatusCodes.Status400BadRequest,
            "TASK_CANNOT_BE_RETRIED")
    {
        TaskId = taskId;
        Status = status;
    }

    public override object ToProblemDetails()
    {
        return new
        {
            Type = $"https://httpstatuses.com/{StatusCode}",
            Title = "Task Cannot Be Retried",
            Status = StatusCode,
            ErrorCode = ErrorCode,
            Detail = Message,
            Instance = Guid.NewGuid().ToString(),
            TaskId = TaskId.ToString(),
            CurrentStatus = Status.ToString()
        };
    }
}