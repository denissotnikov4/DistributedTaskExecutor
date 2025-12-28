using System.ComponentModel.DataAnnotations;

namespace ApiKeys.Client.Models;

public class ApiKeyCreateRequest
{
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Claims { get; set; }
}

