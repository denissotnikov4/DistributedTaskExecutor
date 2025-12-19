namespace TaskService.Client.Models.Tasks;

public class ClientTask
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public ProgrammingLanguage Language { get; set; }

    public string? InputData { get; set; }

    public Guid UserId { get; set; }

    public string? Result { get; set; }

    public TaskStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TimeSpan Ttl { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }
}