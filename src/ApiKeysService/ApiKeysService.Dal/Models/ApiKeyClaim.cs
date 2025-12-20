namespace ApiKeysService.Dal.Models;

public class ApiKeyClaim
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID API-ключа
    /// </summary>
    public Guid ApiKeyId { get; set; }

    /// <summary>
    /// Тип клейма (например, "role", "permission", "userId")
    /// </summary>
    public string ClaimType { get; set; } = string.Empty;

    /// <summary>
    /// Значение клейма
    /// </summary>
    public string ClaimValue { get; set; } = string.Empty;

    /// <summary>
    /// Навигационное свойство
    /// </summary>
    public ApiKey ApiKey { get; set; } = null!;
}