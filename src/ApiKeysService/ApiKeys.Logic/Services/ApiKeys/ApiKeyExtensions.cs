using ApiKeys.Client.Models;
using ApiKeys.Dal.Models;

namespace ApiKeys.Logic.Services.ApiKeys;

public static class ApiKeyExtensions
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