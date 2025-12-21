using ApiKeys.Dal.Data;
using Core.Database;

namespace ApiKeys.Dal.Repositories;

public interface IApiKeysUnitOfWork : IUnitOfWork
{
    IApiKeysRepository ApiKeys { get; }
}

public class ApiKeysUnitOfWork : UnitOfWork, IApiKeysUnitOfWork
{
    public ApiKeysUnitOfWork(
        IApiKeysRepository apiKeysRepository,
        ApiKeyDbContext dbContext) : base(dbContext)
    {
        ApiKeys = apiKeysRepository;
    }

    public IApiKeysRepository ApiKeys { get; }
}