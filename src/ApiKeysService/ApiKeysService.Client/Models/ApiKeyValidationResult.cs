namespace ApiKeysService.Client.Models;

public class ApiKeyValidationResult
{
    public bool IsValid { get; set; }
    public Guid? ApiKeyId { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

