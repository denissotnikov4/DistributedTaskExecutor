using ApiKeysService.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiKeysService.Dal.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");
        
        builder.HasKey(e => e.Id);

        builder.Property(e => e.KeyHash).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(250);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => e.KeyHash).IsUnique();

        builder.Property(e => e.Claims).HasColumnType("text[]");
    }
}

