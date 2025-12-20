using System.Security.Cryptography;
using System.Text;
using ApiKeysService.Client.Models;
using ApiKeysService.Dal.Models;
using ApiKeysService.Dal.Repositories;
using Core.Results;
using Microsoft.Extensions.Logging;

namespace ApiKeysService.Logic.Services;

public interface IApiKeysService
{
    Task<Result<ApiKeyCreateResponse>> CreateApiKeyAsync(ApiKeyCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ApiKeyInfo>> GetApiKeyInfoAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ApiKeyInfo>>> GetAllApiKeysAsync(CancellationToken cancellationToken = default);
    Task<Result> UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteApiKeyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<ApiKeyValidationResult>> ValidateApiKeyAsync(
        string apiKey, CancellationToken cancellationToken = default);
}

public class ApiKeysService(
    IApiKeysRepository repository,
    ILogger<ApiKeysService> logger
)
    : IApiKeysService
{
    private const int KeyLengthInBytes = 32;

    public async Task<Result<ApiKeyCreateResponse>> CreateApiKeyAsync(ApiKeyCreateRequest request,
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
            IsActive = true
        };

        if (request.Claims != null && request.Claims.Count > 0)
        {
            apiKeyEntity.Claims = request.Claims;
        }

        await repository.CreateAsync(apiKeyEntity, cancellationToken);

        var info = apiKeyEntity.MapToInfo();

        logger.LogInformation("Api-Key created with id: {ApiKeyId}", apiKeyEntity.Id);

        return new ApiKeyCreateResponse
        {
            Id = apiKeyEntity.Id,
            ApiKey = fullApiKey, // Возвращаем только при создании
            Info = info
        };
    }

    public async Task<Result<ApiKeyInfo>> GetApiKeyInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var apiKey = await repository.GetByIdAsync(id, cancellationToken);

        if (apiKey == null)
        {
            return ServiceError.NotFound($"Api-Key with id {id} not found");
        }

        return apiKey.MapToInfo();
    }

    public async Task<Result<ICollection<ApiKeyInfo>>> GetAllApiKeysAsync(CancellationToken cancellationToken = default)
    {
        var apiKeys = await repository.GetAllAsync(cancellationToken);
        return apiKeys.Select(x => x.MapToInfo()).ToList();
    }

    public async Task<Result> UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await repository.GetByIdAsync(id, cancellationToken);
        if (apiKey == null)
        {
            return ServiceError.NotFound($"Api-Key with id {id} not found");
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
            apiKey.Claims = request.Claims;
        }

        await repository.UpdateAsync(apiKey, cancellationToken);
        logger.LogInformation("Api-Key updated with id: {ApiKeyId}", id);
        
        return Result.Success;
    }

    public async Task<Result> DeleteApiKeyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await repository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Api-Key deleted with id: {ApiKeyId}", id);
        
        return Result.Success;
    }

    public async Task<Result<ApiKeyValidationResult>> ValidateApiKeyAsync(
        string apiKey, CancellationToken cancellationToken = default)
    {
        var keyHash = ComputeHash(apiKey);

        var apiKeyEntity = await repository.GetByKeyHashAsync(keyHash, cancellationToken);

        if (apiKeyEntity == null)
        {
            return ServiceError.Conflict("Api-Key not valid");
        }

        if (!apiKeyEntity.IsActive)
        {
            return ServiceError.Conflict("Api-Key is inactive");
        }

        if (apiKeyEntity.ExpiresAt.HasValue && apiKeyEntity.ExpiresAt.Value < DateTime.UtcNow)
        {
            return ServiceError.Conflict("Api-Key has expired");
        }

        await repository.UpdateLastUsedAsync(apiKeyEntity.Id, DateTime.UtcNow, cancellationToken);

        return new ApiKeyValidationResult
        {
            ApiKeyId = apiKeyEntity.Id,
            Claims = apiKeyEntity.Claims
        };
    }

    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

}

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