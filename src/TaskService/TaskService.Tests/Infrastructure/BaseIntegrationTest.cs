using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;
using TaskService.Core.RabbitMQ;
using TaskService.Dal.Data;
using TaskService.Dal.Models;
using TaskService.Dal.Repositories;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;
using Program = TaskService.Api.Program;

namespace TaskService.Tests.Infrastructure;

[TestFixture]
internal abstract class BaseIntegrationTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private Mock<IRabbitMessageQueue<Guid>> _rabbitMqMock = null!;
    
    private static readonly TestContainersFixture _testContainersFixture = new();
    
    protected HttpClient Client => _client;
    protected Mock<IRabbitMessageQueue<Guid>> RabbitMqMock => _rabbitMqMock;
    protected ITaskRepository TaskRepository => _scope.ServiceProvider.GetRequiredService<ITaskRepository>();
    
    public async Task InitializeAsync()
    {
        // Инициализируем Testcontainers (один раз для всех тестов)
        if (!_testContainersFixture.IsInitialized)
        {
            await _testContainersFixture.GlobalSetup();
        }
    }
    
    public async Task DisposeAsync()
    {
        // Очищаем ресурсы после каждого теста
        await CleanupAsync();
    }
    
    [SetUp]
    public virtual async Task SetUp()
    {
        if (!_testContainersFixture.IsInitialized)
        {
            if (!IsDockerAvailable())
            {
                Assert.Ignore("Docker is not available. Skipping integration tests.");
                return;
            }
            
            // Инициализируем контейнер при первом запуске
            await _testContainersFixture.GlobalSetup();
        }
        
        // Очищаем БД перед каждым тестом
        await _testContainersFixture.ResetDatabaseAsync();
        
        _rabbitMqMock = new Mock<IRabbitMessageQueue<Guid>>();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Удаляем существующую регистрацию RabbitMQ
                    var rabbitMqDescriptor = services.FirstOrDefault(
                        d => d.ServiceType == typeof(IRabbitMessageQueue<Guid>));
                    if (rabbitMqDescriptor != null)
                    {
                        services.Remove(rabbitMqDescriptor);
                    }
                    
                    // Добавляем мок
                    services.AddSingleton(_rabbitMqMock.Object);
                    
                    // Удаляем существующий DbContext
                    var dbContextDescriptor = services.FirstOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TaskDbContext>));
                    if (dbContextDescriptor != null)
                    {
                        services.Remove(dbContextDescriptor);
                    }
                    
                    services.AddAuthentication("ApiKey")  // Имя схемы из ApiKeyAuthConstants.SchemeName
                        .AddScheme<AuthenticationSchemeOptions, TestApiKeyHandler>("ApiKey", _ => { });

                    
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("ApiKeyClaims_tasks:write", policy => 
                            policy.RequireAssertion(_ => true));  // Разрешаем всегда
        
                        options.AddPolicy("ApiKeyClaims_tasks:read", policy => 
                            policy.RequireAssertion(_ => true));
                        
                        options.AddPolicy("ApiKeyRequired", policy => 
                            policy.RequireAssertion(_ => true));
                        
                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .RequireAssertion(_ => true)
                            .Build();
                    });
                    
                    // Добавляем реальную PostgreSQL БД из контейнера
                    services.AddDbContext<TaskDbContext>(options =>
                    {
                        options.UseNpgsql(_testContainersFixture.ConnectionString);
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    });
                });
                
                // Используем тестовое окружение
                builder.UseEnvironment("Test");
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }
    
    [TearDown]
    public virtual async Task TearDown()
    {
        await CleanupAsync();
    }
    
    private async Task CleanupAsync()
    {
        await ClearDatabaseAsync();
        
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    private async Task ClearDatabaseAsync()
    {
        await _testContainersFixture.ResetDatabaseAsync();
    }
    
    // Дополнительный статический метод для глобальной очистки
    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        if (_testContainersFixture.IsInitialized)
        {
            await _testContainersFixture.GlobalTeardown();
        }
    }
    
    // Остальные методы остаются без изменений
    protected async Task<Guid> CreateTestTaskInDbAsync(
        Guid? userId = null,
        string? name = null,
        string? code = null,
        ProgrammingLanguage language = ProgrammingLanguage.CSharp,
        TaskStatus status = TaskStatus.Pending,
        string? errorMessage = null,
        int retryCount = 0,
        TimeSpan? ttl = null)
    {
        var task = new ServerTask
        {
            Id = Guid.NewGuid(),
            Name = name ?? "Test Task",
            Code = code ?? "Console.WriteLine(\"Hello\");",
            Language = language,
            InputData = "{}",
            UserId = userId ?? Guid.NewGuid(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            Ttl = ttl ?? TimeSpan.FromHours(1),
            RetryCount = retryCount,
            ErrorMessage = errorMessage
        };

        switch (status)
        {
            case TaskStatus.Completed:
                task.CompletedAt = DateTime.UtcNow;
                task.StartedAt = DateTime.UtcNow.AddMinutes(-5);
                task.Result = "{\"output\": \"Hello\\n\"}";
                break;
            case TaskStatus.Failed:
                task.CompletedAt = DateTime.UtcNow;
                task.StartedAt = DateTime.UtcNow.AddMinutes(-5);
                task.ErrorMessage = errorMessage ?? "Compilation error";
                break;
            case TaskStatus.InProgress:
                task.StartedAt = DateTime.UtcNow;
                break;
            case TaskStatus.Expired:
                task.StartedAt = DateTime.UtcNow.AddHours(-2);
                task.CompletedAt = DateTime.UtcNow.AddHours(-2);
                task.ErrorMessage = "Task expired";
                break;
        }

        await TaskRepository.CreateAsync(task);

        return task.Id;
    }
    
    protected TaskCreateRequest CreateValidTaskRequest()
    {
        return new TaskCreateRequest
        {
            Name = "Test Task",
            Code = "using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(\"Hello World\");\n    }\n}",
            Language = ProgrammingLanguage.CSharp,
            InputData = "{\"test\": \"data\"}",
            Ttl = TimeSpan.FromMinutes(30)
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