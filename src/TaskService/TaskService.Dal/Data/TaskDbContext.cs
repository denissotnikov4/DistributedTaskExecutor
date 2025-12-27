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
            entity.HasKey(task=> task.Id);
            entity.Property(task=> task.Name).IsRequired().HasMaxLength(250);
            entity.Property(task=> task.Code).IsRequired();
            entity.Property(task => task.Language).IsRequired();
            entity.Property(task=> task.InputData).IsRequired(false);
            entity.Property(task=> task.UserId).IsRequired();
            entity.Property(task=> task.Result).IsRequired(false);
            entity.Property(task=> task.Status).IsRequired();
            entity.Property(task=> task.CreatedAt).IsRequired();
            entity.Property(task=> task.StartedAt).IsRequired(false);
            entity.Property(task=> task.CompletedAt).IsRequired(false);
            entity.Property(task=> task.Ttl).IsRequired();
            entity.Property(task=> task.ErrorMessage).IsRequired(false);
            entity.Property(task=> task.RetryCount).HasDefaultValue(0);
        });
    }
}