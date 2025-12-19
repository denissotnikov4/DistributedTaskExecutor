using Microsoft.EntityFrameworkCore;
using TaskService.Dal.Models;

namespace TaskService.Dal.Data;

public class TaskDbContext : DbContext
{
    public DbSet<ServerTask> Tasks { get; set; }

    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ServerTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.InputData).IsRequired(false);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Result).IsRequired(false);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.StartedAt).IsRequired(false);
            entity.Property(e => e.CompletedAt).IsRequired(false);
            entity.Property(e => e.Ttl).IsRequired();
            entity.Property(e => e.ErrorMessage).IsRequired(false);
            entity.Property(e => e.RetryCount).HasDefaultValue(0);
        });
    }
}