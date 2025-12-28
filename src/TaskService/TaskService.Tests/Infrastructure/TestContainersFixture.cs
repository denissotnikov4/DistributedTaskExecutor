using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using TaskService.Dal.Data;

namespace TaskService.Tests.Infrastructure;

[SetUpFixture]
internal class TestContainersFixture
{
    private PostgreSqlContainer postgresContainer = null!;
    private Respawner respawner = null!;
    private DbConnection dbConnection = null!;

    public string ConnectionString { get; private set; } = null!;

    public bool IsInitialized { get; private set; } = false;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        if (!IsDockerAvailable())
        {
            Console.WriteLine("⚠Docker is not available. Integration tests will be skipped.");
            return;
        }

        postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("taskservice_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();
        
        await postgresContainer.StartAsync();
        ConnectionString = postgresContainer.GetConnectionString();
        
        Console.WriteLine($"Test database started: {ConnectionString}");

        await ApplyMigrationsAsync();

        await SetupRespawnAsync();
        
        IsInitialized = true;
    }
    
    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (IsInitialized)
        {
            if (this.dbConnection != null)
            {
                await this.dbConnection.CloseAsync();
                await this.dbConnection.DisposeAsync();
            }
            
            if (this.postgresContainer != null)
            {
                await this.postgresContainer.DisposeAsync();
            }
            
            Console.WriteLine("Test database stopped");
        }
    }
    
    public async Task ResetDatabaseAsync()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Test container is not initialized");
        }
        
        await this.respawner.ResetAsync(dbConnection);
    }
    
    private async Task ApplyMigrationsAsync()
    {
        await using var dbContext = CreateDbContext();
        
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await dbContext.Database.EnsureDeletedAsync();
            
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Database migrations applied");
        }
        else
        {
            Console.WriteLine("No pending migrations to apply");
        }
        Console.WriteLine("Database migrations applied");
    }
    
    private async Task SetupRespawnAsync()
    {
        dbConnection = new NpgsqlConnection(ConnectionString);
        await dbConnection.OpenAsync();
        
        respawner = await Respawner.CreateAsync(this.dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
            WithReseed = true
        });
    }
    
    private TaskDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseNpgsql(ConnectionString, builder => builder.MigrationsAssembly("TaskService.Dal"))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;
        
        return new TaskDbContext(options);
    }

    private bool IsDockerAvailable()
    {
        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}