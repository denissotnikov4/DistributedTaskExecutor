using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Dal.Models;

public class ServerTask
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public string? Data { get; set; }

    public Guid UserId { get; set; }

    public string? Result { get; set; }

    public TaskStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TimeSpan Ttl { get; set; }

    public string? ErrorMessage { get; set; }

    public string? WorkerId { get; set; }

    public int RetryCount { get; set; }
}