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
    Task<Result<ApiKeyCreateResponse>> CreateApiKeyAsync(ApiKeyCreateRequest request);
    Task<Result<ApiKeyInfo>> GetApiKeyInfoAsync(Guid id);
    Task<Result<ICollection<ApiKeyInfo>>> GetAllApiKeysAsync();
    Task<Result> UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request);
    Task<Result> DeleteApiKeyAsync(Guid id);
    Task<Result<ApiKeyValidationResult>> ValidateApiKeyAsync(string apiKey);
}

public class ApiKeysService(
    IApiKeysUnitOfWork unitOfWork,
    ILogger<ApiKeysService> logger
)
    : IApiKeysService
{
    private const int KeyLengthInBytes = 32;

    public async Task<Result<ApiKeyCreateResponse>> CreateApiKeyAsync(ApiKeyCreateRequest request)
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

        unitOfWork.ApiKeys.Create(apiKeyEntity);
        await unitOfWork.SaveChangesAsync();

        var info = apiKeyEntity.MapToInfo();

        logger.LogInformation("Api-Key created with id: {ApiKeyId}", apiKeyEntity.Id);

        return new ApiKeyCreateResponse
        {
            Id = apiKeyEntity.Id,
            ApiKey = fullApiKey, // Возвращаем только при создании
            Info = info
        };
    }

    public async Task<Result<ApiKeyInfo>> GetApiKeyInfoAsync(Guid id)
    {
        var apiKey = await unitOfWork.ApiKeys.GetByIdAsync(id);

        if (apiKey == null)
        {
            return ServiceError.NotFound($"Api-Key with id {id} not found");
        }

        return apiKey.MapToInfo();
    }

    public async Task<Result<ICollection<ApiKeyInfo>>> GetAllApiKeysAsync()
    {
        var apiKeys = await unitOfWork.ApiKeys.GetAllAsync();
        return apiKeys.Select(x => x.MapToInfo()).ToList();
    }

    public async Task<Result> UpdateApiKeyAsync(Guid id, ApiKeyUpdateRequest request)
    {
        var apiKey = await unitOfWork.ApiKeys.GetByIdAsync(id);
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

        unitOfWork.ApiKeys.Update(apiKey);
        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Api-Key updated with id: {ApiKeyId}", id);
        
        return Result.Success;
    }

    public async Task<Result> DeleteApiKeyAsync(Guid id)
    {
        await unitOfWork.ApiKeys.DeleteAsync(id);
        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Api-Key deleted with id: {ApiKeyId}", id);
        
        return Result.Success;
    }

    public async Task<Result<ApiKeyValidationResult>> ValidateApiKeyAsync(string apiKey)
    {
        var keyHash = ComputeHash(apiKey);

        var apiKeyEntity = await unitOfWork.ApiKeys.GetByKeyHashAsync(keyHash);

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

        await unitOfWork.ApiKeys.UpdateLastUsedAsync(apiKeyEntity.Id, DateTime.UtcNow);
        await unitOfWork.SaveChangesAsync();

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