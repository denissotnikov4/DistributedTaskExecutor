using ApiKeysService.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiKeysService.Dal.Data;

public class ApiKeyDbContext : DbContext
{
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<ApiKeyClaim> ApiKeyClaims { get; set; }

    public ApiKeyDbContext(DbContextOptions<ApiKeyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KeyHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.ClaimsJson).IsRequired(false);

            // Индекс для быстрого поиска по хэшу
            entity.HasIndex(e => e.KeyHash).IsUnique();

            // Связь один-ко-многим с клеймами
            entity.HasMany(e => e.Claims)
                .WithOne(c => c.ApiKey)
                .HasForeignKey(c => c.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiKeyClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClaimValue).IsRequired().HasMaxLength(500);

            // Индекс для быстрого поиска
            entity.HasIndex(e => new { e.ApiKeyId, e.ClaimType });
        });
    }
}