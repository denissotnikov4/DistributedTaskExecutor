using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ApiKeysService.Client.Models;
using ApiKeysService.Dal.Models;
using ApiKeysService.Dal.Repositories;

namespace ApiKeysService.Logic.Services;

public interface IApiKeysService
{
    Task<ApiKeyCreateResponse> CreateApiKeyAsync(ApiKeyCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<ApiKeyInfo>> GetAllApiKeysAsync(CancellationToken cancellationToken = default);
    Task UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteApiKeyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
}

public class ApiKeysService(IApiKeysRepository repository) : IApiKeysService
{
    private const int KeyLengthInBytes = 32;

    public async Task<ApiKeyCreateResponse> CreateApiKeyAsync(ApiKeyCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKeyBytes = new byte[KeyLengthInBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(apiKeyBytes);
        }

        var apiKey = Convert.ToBase64String(apiKeyBytes);

        var keyPrefix = $"ak_{Guid.NewGuid():N}";
        var fullApiKey = $"{keyPrefix}_{apiKey}";

        var keyHash = ComputeHash(fullApiKey);

        var apiKeyEntity = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = keyHash,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsActive = true,
            ClaimsJson = request.Claims != null ? JsonSerializer.Serialize(request.Claims) : null
        };

        if (request.Claims != null)
        {
            foreach (var claim in request.Claims)
            {
                apiKeyEntity.Claims.Add(new ApiKeyClaim
                {
                    Id = Guid.NewGuid(),
                    ApiKeyId = apiKeyEntity.Id,
                    ClaimType = claim.Key,
                    ClaimValue = claim.Value
                });
            }
        }

        await repository.CreateAsync(apiKeyEntity, cancellationToken);

        var info = MapToInfo(apiKeyEntity);

        return new ApiKeyCreateResponse
        {
            Id = apiKeyEntity.Id,
            ApiKey = fullApiKey, // Возвращаем только при создании
            Info = info
        };
    }

    public async Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var apiKey = await repository.GetByIdAsync(id, cancellationToken);
        return apiKey != null ? MapToInfo(apiKey) : null;
    }

    public async Task<ICollection<ApiKeyInfo>> GetAllApiKeysAsync(CancellationToken cancellationToken = default)
    {
        var apiKeys = await repository.GetAllAsync(cancellationToken);
        return apiKeys.Select(MapToInfo).ToList();
    }

    public async Task UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await repository.GetByIdAsync(id, cancellationToken);
        if (apiKey == null)
        {
            throw new KeyNotFoundException($"API key with id {id} not found");
        }

        if (request.Name != null)
        {
            apiKey.Name = request.Name;
        }

        if (request.ExpiresAt.HasValue)
        {
            apiKey.ExpiresAt = request.ExpiresAt;
        }

        if (request.IsActive.HasValue)
        {
            apiKey.IsActive = request.IsActive.Value;
        }

        if (request.Claims != null)
        {
            // Удаляем старые клеймы
            apiKey.Claims.Clear();

            // Добавляем новые клеймы
            foreach (var claim in request.Claims)
            {
                apiKey.Claims.Add(new ApiKeyClaim
                {
                    Id = Guid.NewGuid(),
                    ApiKeyId = apiKey.Id,
                    ClaimType = claim.Key,
                    ClaimValue = claim.Value
                });
            }

            apiKey.ClaimsJson = JsonSerializer.Serialize(request.Claims);
        }

        await repository.UpdateAsync(apiKey, cancellationToken);
    }

    public async Task DeleteApiKeyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API key is empty"
            };
        }

        // Хэшируем предоставленный ключ
        var keyHash = ComputeHash(apiKey);

        // Ищем ключ в базе
        var apiKeyEntity = await repository.GetByKeyHashAsync(keyHash, cancellationToken);

        if (apiKeyEntity == null)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API key not found"
            };
        }

        // Проверяем активность
        if (!apiKeyEntity.IsActive)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API key is inactive"
            };
        }

        // Проверяем срок действия
        if (apiKeyEntity.ExpiresAt.HasValue && apiKeyEntity.ExpiresAt.Value < DateTime.UtcNow)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API key has expired"
            };
        }

        await repository.UpdateLastUsedAsync(apiKeyEntity.Id, DateTime.UtcNow, cancellationToken);

        var claims = apiKeyEntity.Claims.ToDictionary(c => c.ClaimType, c => c.ClaimValue);

        return new ApiKeyValidationResult
        {
            IsValid = true,
            ApiKeyId = apiKeyEntity.Id,
            Claims = claims
        };
    }

    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    private static ApiKeyInfo MapToInfo(ApiKey apiKey)
    {
        var claims = apiKey.Claims.ToDictionary(c => c.ClaimType, c => c.ClaimValue);

        return new ApiKeyInfo
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt,
            IsActive = apiKey.IsActive,
            LastUsedAt = apiKey.LastUsedAt,
            Claims = claims
        };
    }
}