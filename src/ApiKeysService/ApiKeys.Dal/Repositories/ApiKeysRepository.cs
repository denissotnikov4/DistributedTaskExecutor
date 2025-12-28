using ApiKeys.Dal.Data;
using ApiKeys.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiKeys.Dal.Repositories;

public interface IApiKeysRepository
{
    Task<ApiKey?> GetByIdAsync(Guid id);
    Task<ApiKey?> GetByKeyHashAsync(string keyHash);
    Task<ICollection<ApiKey>> GetAllAsync();
    Guid Create(ApiKey apiKey);
    void Update(ApiKey apiKey);
    void Delete(ApiKey apiKey);
    Task UpdateLastUsedAsync(Guid id, DateTime lastUsedAt);
}

public class ApiKeysRepository(ApiKeyDbContext context) : IApiKeysRepository
{
    public async Task<ApiKey?> GetByIdAsync(Guid id)
    {
        return await context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<ApiKey?> GetByKeyHashAsync(string keyHash)
    {
        return await context.ApiKeys.FirstOrDefaultAsync(k => k.KeyHash == keyHash);
    }

    public async Task<ICollection<ApiKey>> GetAllAsync()
    {
        return await context.ApiKeys.ToListAsync();
    }

    public Guid Create(ApiKey apiKey)
    {
        context.ApiKeys.Add(apiKey);
        return apiKey.Id;
    }

    public void Update(ApiKey apiKey)
    {
        context.ApiKeys.Update(apiKey);
    }

    public void Delete(ApiKey apiKey)
    {
        context.ApiKeys.Remove(apiKey);
    }

    public async Task UpdateLastUsedAsync(Guid id, DateTime lastUsedAt)
    {
        var apiKey = await context.ApiKeys.FindAsync(id);
        if (apiKey != null)
        {
            apiKey.LastUsedAt = lastUsedAt;
            context.ApiKeys.Update(apiKey);
        }
    }
}