using Core.Database;
using Microsoft.EntityFrameworkCore;

namespace ApiKeysService.Dal.Repositories;

public interface IApiKeysUnitOfWork : IUnitOfWork
{
    IApiKeysRepository ApiKeys { get; }
}

public class ApiKeysUnitOfWork : UnitOfWork, IApiKeysUnitOfWork
{
    public ApiKeysUnitOfWork(
        IApiKeysRepository apiKeysRepository,
        DbContext dbContext) : base(dbContext)
    {
        ApiKeys = apiKeysRepository;
    }

    public IApiKeysRepository ApiKeys { get; }
}