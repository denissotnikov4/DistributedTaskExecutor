namespace TaskService.Client.Tasks;

public class ClientTask
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string Code { get; set; } = null!;

    public string? Input { get; set; }

    public string? Output { get; set; }

    public TaskStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TimeSpan Ttl { get; set; }

    public string? ErrorMessage { get; set; }

    public string? WorkerId { get; set; }
}