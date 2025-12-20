namespace ApiKeysService.Client.Models;

public class ApiKeyCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Claims { get; set; }
}

