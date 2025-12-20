namespace ApiKeysService.Client.Models;

public class ApiKeyCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}

