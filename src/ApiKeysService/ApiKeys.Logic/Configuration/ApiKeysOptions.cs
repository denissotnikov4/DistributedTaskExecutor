namespace ApiKeys.Logic.Configuration;

/// <summary>
/// Опции конфигурации для ApiKeysService
/// </summary>
public class ApiKeysOptions
{
    /// <summary>
    /// Список разрешенных claims, которые можно использовать при создании API-ключей.
    /// Если пусто, разрешены любые claims (только для администраторов).
    /// </summary>
    public List<string> AllowedClaims { get; set; } = new();

    /// <summary>
    /// Требовать, чтобы пользователь имел в своем JWT токене те же claims, которые он пытается установить в API-ключе.
    /// По умолчанию false - только администраторы могут устанавливать любые claims.
    /// </summary>
    public bool RequireMatchingClaims { get; set; } = false;
}

