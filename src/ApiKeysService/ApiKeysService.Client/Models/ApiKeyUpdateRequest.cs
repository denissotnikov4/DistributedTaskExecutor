namespace ApiKeysService.Client.Models;

public class ApiKeyUpdateRequest
{
    public string? Name { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Claims { get; set; }
}

