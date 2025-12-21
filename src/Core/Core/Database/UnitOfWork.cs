using Microsoft.EntityFrameworkCore;

namespace Core.Database;

public abstract class UnitOfWork(DbContext dbContext) : IUnitOfWork
{
    private bool disposed;

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                dbContext.DisposeAsync().AsTask().Wait();
            }

            disposed = true;
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}