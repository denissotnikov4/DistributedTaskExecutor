namespace ApiKeysService.Client.Models;

public class ApiKeyValidationResult
{
    public Guid? ApiKeyId { get; set; }
    public List<string> Claims { get; set; } = new();
}

