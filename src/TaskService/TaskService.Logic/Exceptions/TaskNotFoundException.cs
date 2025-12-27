using Microsoft.AspNetCore.Http;
using TaskService.Logic.Exceptions.Base;

namespace TaskService.Logic.Exceptions;

internal class TaskNotFoundException : TaskServiceException
{
    private Guid TaskId { get; }

    public TaskNotFoundException(Guid taskId)
        : base($"Task with id {taskId} not found.", StatusCodes.Status404NotFound, "TASK_NOT_FOUND")
    {
        TaskId = taskId;
    }

    public override object ToProblemDetails()
    {
        return new
        {
            Type = $"https://httpstatuses.com/{StatusCode}",
            Title = "Task Not Found",
            Status = StatusCode,
            ErrorCode = ErrorCode,
            Detail = Message,
            Instance = Guid.NewGuid().ToString(),
            TaskId = TaskId.ToString()
        };
    }
}