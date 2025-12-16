namespace TaskService.Client.Models.Requests;

public class CreateTaskRequest
{
    public string Description { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public string? Code { get; set; }

    public TimeSpan Ttl { get; set; }

    public int MaxRetries { get; set; } = 3;
}