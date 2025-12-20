namespace ApiKeysService.Client.Models;

public class ApiKeyUpdateRequest
{
    public string? Name { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsActive { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}

