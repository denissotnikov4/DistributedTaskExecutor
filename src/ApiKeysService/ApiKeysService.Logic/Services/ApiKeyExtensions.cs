using ApiKeysService.Client.Models;
using ApiKeysService.Dal.Models;

namespace ApiKeysService.Logic.Services;

internal static class ApiKeyExtensions
{
    public static ApiKeyInfo MapToInfo(this ApiKey apiKey)
    {
        return new ApiKeyInfo
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt,
            IsActive = apiKey.IsActive,
            LastUsedAt = apiKey.LastUsedAt,
            Claims = apiKey.Claims
        };
    }
}