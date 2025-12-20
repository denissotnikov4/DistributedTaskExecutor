# Интеграция ApiKeysService в существующие сервисы

Этот документ описывает, как интегрировать микросервис управления API-ключами в существующие сервисы (TaskService, WorkerService и т.д.).

## Обзор

ApiKeysService предоставляет централизованное управление API-ключами с поддержкой claims (клеймов). Ключи хранятся в зашифрованном виде (SHA256 хэш), что обеспечивает безопасность.

## Варианты интеграции

### Вариант 1: Прямые HTTP-вызовы к ApiKeysService

Используйте `ApiKeysService.Client` для валидации ключей:

```csharp
// В Program.cs или DI модуле
services.AddApiKeysClient("http://apikeys-service:5000");

// В middleware или атрибуте авторизации
public class ApiKeyAuthenticationMiddleware
{
    private readonly IApiKeysClient apiKeysClient;
    private readonly RequestDelegate next;

    public ApiKeyAuthenticationMiddleware(IApiKeysClient apiKeysClient, RequestDelegate next)
    {
        this.apiKeysClient = apiKeysClient;
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API key is required");
            return;
        }

        var validationResult = await apiKeysClient.ValidateApiKeyAsync(apiKeyHeader.ToString());
        
        if (!validationResult.IsValid)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(validationResult.ErrorMessage ?? "Invalid API key");
            return;
        }

        // Добавляем claims в контекст для использования в контроллерах
        foreach (var claim in validationResult.Claims)
        {
            context.User.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(claim.Key, claim.Value)
            }));
        }

        await next(context);
    }
}
```

### Вариант 2: Custom Authentication Handler

Создайте кастомный AuthenticationHandler для ASP.NET Core:

```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiKeysClient apiKeysClient;

    public ApiKeyAuthenticationHandler(
        IApiKeysClient apiKeysClient,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        this.apiKeysClient = apiKeysClient;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            return AuthenticateResult.Fail("API key is missing");
        }

        var validationResult = await apiKeysClient.ValidateApiKeyAsync(apiKeyHeader.ToString());
        
        if (!validationResult.IsValid)
        {
            return AuthenticateResult.Fail(validationResult.ErrorMessage ?? "Invalid API key");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, validationResult.ApiKeyId.ToString()!)
        };

        foreach (var claim in validationResult.Claims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
```

Зарегистрируйте в Program.cs:

```csharp
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

builder.Services.AddApiKeysClient("http://apikeys-service:5000");
```

Используйте в контроллерах:

```csharp
[Authorize(AuthenticationSchemes = "ApiKey")]
[ApiController]
public class TasksController : ControllerBase
{
    // ...
}
```

### Вариант 3: Кэширование для производительности

Для снижения нагрузки на ApiKeysService можно добавить кэширование:

```csharp
public class CachedApiKeysClient : IApiKeysClient
{
    private readonly IApiKeysClient innerClient;
    private readonly IMemoryCache cache;
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(5);

    public CachedApiKeysClient(IApiKeysClient innerClient, IMemoryCache cache)
    {
        this.innerClient = innerClient;
        this.cache = cache;
    }

    public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"api_key_validation_{apiKey}";
        
        if (cache.TryGetValue<ApiKeyValidationResult>(cacheKey, out var cachedResult))
        {
            return cachedResult!;
        }

        var result = await innerClient.ValidateApiKeyAsync(apiKey, cancellationToken);
        
        cache.Set(cacheKey, result, cacheExpiration);
        
        return result;
    }
    
    // Реализуйте остальные методы, делегируя вызовы innerClient
}
```

## Использование Claims

После валидации API-ключа, claims доступны через `HttpContext.User`:

```csharp
[Authorize(AuthenticationSchemes = "ApiKey")]
[ApiController]
public class TasksController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        // Получить userId из claims
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("User ID claim is missing or invalid");
        }

        // Использовать userId при создании задачи
        // ...
    }
}
```

## Примеры создания API-ключей с Claims

```csharp
// Создать ключ с claims через API
var createRequest = new ApiKeyCreateRequest
{
    Name = "TaskService Production Key",
    ExpiresAt = DateTime.UtcNow.AddYears(1),
    Claims = new Dictionary<string, string>
    {
        { "userId", "123e4567-e89b-12d3-a456-426614174000" },
        { "role", "admin" },
        { "permission", "tasks:create" },
        { "permission", "tasks:read" }
    }
};

var response = await apiKeysClient.CreateApiKeyAsync(createRequest);
// Сохраните response.ApiKey в безопасном месте - он больше не будет показан!
```

## Конфигурация

Добавьте в appsettings.json:

```json
{
  "ApiKeysService": {
    "BaseUrl": "http://apikeys-service:5000"
  }
}
```

И в Program.cs:

```csharp
var apiKeysServiceUrl = builder.Configuration["ApiKeysService:BaseUrl"] 
    ?? throw new InvalidOperationException("ApiKeysService:BaseUrl is required");

builder.Services.AddApiKeysClient(apiKeysServiceUrl);
```

## Безопасность

1. **Никогда не логируйте полные API-ключи** - только их ID или хэш
2. **Используйте HTTPS** для всех коммуникаций между сервисами
3. **Ограничьте доступ** к ApiKeysService только из внутренней сети
4. **Регулярно ротируйте ключи** - удаляйте неиспользуемые ключи
5. **Устанавливайте срок действия** для ключей через `ExpiresAt`

## Мониторинг

Отслеживайте:
- Количество валидаций в секунду
- Процент невалидных ключей
- Время ответа ApiKeysService
- Использование кэша (если используется)

## Обработка ошибок

При недоступности ApiKeysService можно:
1. **Fallback на кэш** - использовать последние известные валидные ключи
2. **Режим обслуживания** - временно отключить проверку ключей (только для разработки!)
3. **Retry с экспоненциальной задержкой** - для временных сбоев

