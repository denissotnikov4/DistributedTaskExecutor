namespace ApiKeys.Dal.Models;

public class ApiKey
{
    public Guid Id { get; set; }

    /// <summary>
    /// Хэш API-ключа (хранится вместо самого ключа)
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Название/описание ключа
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания ключа
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата истечения ключа (null = бессрочный)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Активен ли ключ
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Последнее использование ключа
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Клеймы (claims) ключа как массив строк
    /// </summary>
    public List<string> Claims { get; set; } = new();
}