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
        if (!this.disposed)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }

            this.disposed = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                dbContext.DisposeAsync().AsTask().Wait();
            }

            this.disposed = true;
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}