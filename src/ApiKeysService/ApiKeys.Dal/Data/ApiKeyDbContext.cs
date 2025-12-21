using ApiKeys.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiKeys.Dal.Data;

public class ApiKeyDbContext(DbContextOptions<ApiKeyDbContext> options) : DbContext(options)
{
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiKeyDbContext).Assembly);
    }
}