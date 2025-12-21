namespace ApiKeys.Client.Models;

public class ApiKeyCreateResponse
{
    public Guid Id { get; set; }
    public string ApiKey { get; set; } = string.Empty; // Возвращается только при создании
    public ApiKeyInfo Info { get; set; } = null!;
}

