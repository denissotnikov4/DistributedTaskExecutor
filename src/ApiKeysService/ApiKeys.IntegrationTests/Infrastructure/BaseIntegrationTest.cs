using ApiKeys.Api;
using ApiKeys.Client.Models;
using ApiKeys.Dal.Data;
using ApiKeys.Dal.Models;
using ApiKeys.Dal.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeys.IntegrationTests.Infrastructure;

[TestFixture]
public abstract class BaseIntegrationTest
{
    private const string TestEnvironmentName = "Test";

    private WebApplicationFactory<Program> factory = null!;
    private IServiceScope scope = null!;

    private static readonly TestContainersFixture TestContainersFixture = new();

    protected HttpClient Client { get; private set; } = null!;

    protected IApiKeysRepository ApiKeysRepository =>
        this.scope.ServiceProvider.GetRequiredService<IApiKeysRepository>();

    public async Task InitializeAsync()
    {
        if (!TestContainersFixture.IsInitialized)
        {
            await TestContainersFixture.GlobalSetup();
        }
    }

    public async Task DisposeAsync()
    {
        await this.CleanupAsync();
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        if (!TestContainersFixture.IsInitialized)
        {
            if (!this.IsDockerAvailable())
            {
                Assert.Ignore("Docker is not available. Skipping integration tests.");
                return;
            }

            await TestContainersFixture.GlobalSetup();
        }

        await TestContainersFixture.ResetDatabaseAsync();

        this.factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Удаляем существующие регистрации DbContext
                    var dbContextDescriptor =
                        services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApiKeyDbContext>));
                    if (dbContextDescriptor != null)
                    {
                        services.Remove(dbContextDescriptor);
                    }

                    var dbContextServiceDescriptor =
                        services.FirstOrDefault(d => d.ServiceType == typeof(ApiKeyDbContext));
                    if (dbContextServiceDescriptor != null)
                    {
                        services.Remove(dbContextServiceDescriptor);
                    }

                    // Удаляем существующие регистрации аутентификации
                    var authDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAuthenticationService));
                    if (authDescriptor != null)
                    {
                        services.Remove(authDescriptor);
                    }

                    services.AddAuthentication("TestApiKey")
                        .AddScheme<AuthenticationSchemeOptions, TestApiKeyHandler>("TestApiKey", _ => { });


                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("ManageApiKey", policy =>
                            policy.RequireAssertion(_ => true));

                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .RequireAssertion(_ => true)
                            .Build();
                    });

                    services.AddDbContext<ApiKeyDbContext>(options =>
                    {
                        options.UseNpgsql(TestContainersFixture.ConnectionString,
                            optionsBuilder => optionsBuilder.MigrationsAssembly("ApiKeys.Dal"));
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    });
                });

                builder.UseEnvironment(TestEnvironmentName);
            });

        this.Client = this.factory.CreateClient();
        this.scope = this.factory.Services.CreateScope();
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        await this.CleanupAsync();
    }

    private async Task CleanupAsync()
    {
        if (TestContainersFixture.IsInitialized)
        {
            await this.ClearDatabaseAsync();
        }

        this.scope.Dispose();
        this.Client.Dispose();
        await this.factory.DisposeAsync();
    }

    private async Task ClearDatabaseAsync()
    {
        if (TestContainersFixture.IsInitialized)
        {
            await TestContainersFixture.ResetDatabaseAsync();
        }
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        if (TestContainersFixture.IsInitialized)
        {
            await TestContainersFixture.GlobalTeardown();
        }
    }

    protected async Task<Guid> CreateTestApiKeyInDbAsync(
        string? name = null,
        DateTime? expiresAt = null,
        bool isActive = true,
        List<string>? claims = null,
        DateTime? lastUsedAt = null)
    {
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = $"test_hash_{Guid.NewGuid()}",
            Name = name ?? "Test ApiKey",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = isActive,
            LastUsedAt = lastUsedAt,
            Claims = claims ?? new List<string>()
        };

        this.ApiKeysRepository.Create(apiKey);
        var unitOfWork = this.scope.ServiceProvider.GetRequiredService<IApiKeysUnitOfWork>();
        await unitOfWork.SaveChangesAsync();

        return apiKey.Id;
    }

    protected ApiKeyCreateRequest CreateValidApiKeyRequest()
    {
        return new ApiKeyCreateRequest
        {
            Name = "Test ApiKey",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Claims = ["tasks:read", "tasks:write"]
        };
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