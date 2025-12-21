using ApiKeys.Client.Models;
using ApiKeys.Dal.Models;
using ApiKeys.Dal.Repositories;
using Core.Results;
using Microsoft.Extensions.Logging;

namespace ApiKeys.Logic.Services;

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
    IApiKeyGenerator apiKeyGenerator,
    ILogger<ApiKeysService> logger
) : IApiKeysService
{
    public async Task<Result<ApiKeyCreateResponse>> CreateApiKeyAsync(ApiKeyCreateRequest request)
    {
        var apiKey = apiKeyGenerator.Generate();

        var apiKeyEntity = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = ApiKeysHasher.ComputeHash(apiKey),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsActive = true
        };

        if (request.Claims is { Count: > 0 })
        {
            apiKeyEntity.Claims = request.Claims;
        }

        unitOfWork.ApiKeys.Create(apiKeyEntity);
        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Api-Key created with id: {ApiKeyId}", apiKeyEntity.Id);

        return new ApiKeyCreateResponse
        {
            Id = apiKeyEntity.Id,
            ApiKey = apiKey,
            Info = apiKeyEntity.MapToInfo()
        };
    }

    public async Task<Result<ApiKeyInfo>> GetApiKeyInfoAsync(Guid id)
    {
        var apiKey = await unitOfWork.ApiKeys.GetByIdAsync(id);

        if (apiKey is null)
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
        if (apiKey is null)
        {
            return ServiceError.NotFound($"Api-Key with id {id} not found");
        }

        UpdateApiKeyProperties(apiKey, request);

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
        var apiKeyEntity = await unitOfWork.ApiKeys.GetByKeyHashAsync(ApiKeysHasher.ComputeHash(apiKey));

        if (apiKeyEntity is null)
        {
            return ServiceError.Conflict("Api-Key not valid");
        }

        var validationResult = ValidateApiKeyStatus(apiKeyEntity);
        if (!validationResult.IsSuccess)
        {
            return validationResult.Error;
        }

        await unitOfWork.ApiKeys.UpdateLastUsedAsync(apiKeyEntity.Id, DateTime.UtcNow);
        await unitOfWork.SaveChangesAsync();

        return new ApiKeyValidationResult
        {
            ApiKeyId = apiKeyEntity.Id,
            Claims = apiKeyEntity.Claims
        };
    }

    private static void UpdateApiKeyProperties(ApiKey apiKey, ApiKeyUpdateRequest request)
    {
        if (request.Name is not null)
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

        if (request.Claims is not null)
        {
            apiKey.Claims = request.Claims;
        }
    }

    private static Result ValidateApiKeyStatus(ApiKey apiKey)
    {
        if (!apiKey.IsActive)
        {
            return ServiceError.Conflict("Api-Key is inactive");
        }

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
        {
            return ServiceError.Conflict("Api-Key has expired");
        }

        return Result.Success;
    }
}