namespace Core.Database;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task SaveChangesAsync();
}