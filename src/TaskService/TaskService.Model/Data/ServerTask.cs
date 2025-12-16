using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Model.Data;

public class ServerTask
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public TaskStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public TimeSpan Ttl { get; set; }

    public string? Result { get; set; }

    public string? ErrorMessage { get; set; }

    public string? WorkerId { get; set; }

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; }

    public string? Code { get; set; }
}