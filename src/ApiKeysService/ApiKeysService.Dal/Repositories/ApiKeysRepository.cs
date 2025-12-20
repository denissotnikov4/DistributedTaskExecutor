using ApiKeysService.Dal.Data;
using ApiKeysService.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiKeysService.Dal.Repositories;

public interface IApiKeysRepository
{
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiKey?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default);
    Task<ICollection<ApiKey>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateLastUsedAsync(Guid id, DateTime lastUsedAt, CancellationToken cancellationToken = default);
}

public class ApiKeysRepository(ApiKeyDbContext context) : IApiKeysRepository
{
    public async Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ApiKeys
            .Include(k => k.Claims)
            .FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
    }

    public async Task<ApiKey?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await context.ApiKeys
            .Include(k => k.Claims)
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash, cancellationToken);
    }

    public async Task<ICollection<ApiKey>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ApiKeys
            .Include(k => k.Claims)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        await context.ApiKeys.AddAsync(apiKey, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return apiKey.Id;
    }

    public async Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        context.ApiKeys.Update(apiKey);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetByIdAsync(id, cancellationToken);
        if (apiKey != null)
        {
            context.ApiKeys.Remove(apiKey);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateLastUsedAsync(Guid id, DateTime lastUsedAt, CancellationToken cancellationToken = default)
    {
        var apiKey = await context.ApiKeys.FindAsync([id], cancellationToken);
        if (apiKey != null)
        {
            apiKey.LastUsedAt = lastUsedAt;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}