namespace TaskManagement.Application.DTOs;

public class CreateTaskDto
{
    public string Description { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? Code { get; set; } // C# код для выполнения
    public TimeSpan Ttl { get; set; }
    public int MaxRetries { get; set; } = 3;
}

