using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TaskService.Model.Data;

namespace TaskService.Model.Migrations;

[DbContext(typeof(TaskDbContext))]
internal class TaskDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.UseIdentityByDefaultColumns();

        modelBuilder.Entity<ServerTask>(b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<string>("Code")
                .HasColumnType("text");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTime?>("CompletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            b.Property<string>("ErrorMessage")
                .HasColumnType("text");

            b.Property<DateTime?>("ExpiresAt")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("MaxRetries")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasDefaultValue(3);

            b.Property<string>("Payload")
                .IsRequired()
                .HasColumnType("text");

            b.Property<int>("RetryCount")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasDefaultValue(0);

            b.Property<string>("Result")
                .HasColumnType("text");

            b.Property<DateTime?>("StartedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("Status")
                .HasColumnType("integer");

            b.Property<TimeSpan>("Ttl")
                .HasColumnType("interval");

            b.Property<string>("WorkerId")
                .HasColumnType("text");

            b.HasKey("Id");

            b.HasIndex("ExpiresAt");

            b.HasIndex("Status");

            b.ToTable("Tasks");
        });
    }
}