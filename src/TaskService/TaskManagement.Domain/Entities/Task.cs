namespace TaskManagement.Domain.Entities;

public class Task
{
    public Guid Id { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public string Payload { get; set; } = string.Empty; // JSON данные для обработки
    
    public TaskStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public TimeSpan Ttl { get; set; } // Максимальное время на выполнение
    
    public string? Result { get; set; } // JSON результат выполнения
    
    public string? ErrorMessage { get; set; }
    
    public string? WorkerId { get; set; } // ID воркера, выполняющего задачу
    
    public int RetryCount { get; set; }
    
    public int MaxRetries { get; set; }
    
    public string? Code { get; set; } // C# код для выполнения
    
    
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    
    public bool CanBeExecuted => Status is TaskStatus.Pending or TaskStatus.Failed;
}

