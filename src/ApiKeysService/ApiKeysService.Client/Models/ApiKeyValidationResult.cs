namespace ApiKeysService.Client.Models;

public class ApiKeyValidationResult
{
    public Guid? ApiKeyId { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

