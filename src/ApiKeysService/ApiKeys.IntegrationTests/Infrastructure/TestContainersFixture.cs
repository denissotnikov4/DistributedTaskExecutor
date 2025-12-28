using System.Data.Common;
using ApiKeys.Dal.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace ApiKeys.IntegrationTests.Infrastructure;

[SetUpFixture]
public class TestContainersFixture
{
    private PostgreSqlContainer postgresContainer = null!;
    private Respawner respawner = null!;
    private DbConnection dbConnection = null!;

    public string ConnectionString { get; private set; } = null!;

    public bool IsInitialized { get; private set; }

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        if (!this.IsDockerAvailable())
        {
            Console.WriteLine("Docker is not available. Integration tests will be skipped.");
            return;
        }

        this.postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("apikeys_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        await this.postgresContainer.StartAsync();
        this.ConnectionString = this.postgresContainer.GetConnectionString();

        Console.WriteLine($"Test database started: {this.ConnectionString}");

        await this.ApplyMigrationsAsync();

        await this.SetupRespawnAsync();

        this.IsInitialized = true;
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (this.IsInitialized)
        {
            await this.dbConnection.CloseAsync();
            await this.dbConnection.DisposeAsync();
            await this.postgresContainer.DisposeAsync();

            Console.WriteLine("Test database stopped");
        }
    }

    public async Task ResetDatabaseAsync()
    {
        if (!this.IsInitialized)
        {
            throw new InvalidOperationException("Test container is not initialized");
        }

        await this.respawner.ResetAsync(this.dbConnection);
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var dbContext = this.CreateDbContext();

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
        this.dbConnection = new NpgsqlConnection(this.ConnectionString);
        await this.dbConnection.OpenAsync();

        this.respawner = await Respawner.CreateAsync(this.dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
            WithReseed = true
        });
    }

    private ApiKeyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApiKeyDbContext>()
            .UseNpgsql(this.ConnectionString, builder => builder.MigrationsAssembly("ApiKeys.Dal"))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        return new ApiKeyDbContext(options);
    }

    private bool IsDockerAvailable()
    {
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
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