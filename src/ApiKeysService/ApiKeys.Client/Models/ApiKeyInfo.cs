namespace ApiKeys.Client.Models;

public class ApiKeyInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public List<string> Claims { get; set; } = new();
}

