# ApiKeysService

Сервис для управления API-ключами в распределенной системе. Предоставляет функциональность создания, управления и валидации API-ключей с поддержкой claims для авторизации.

## Особенности

- API-ключи хранятся в виде хэшей, оригинальные ключи возвращаются только при создании
- Поддержка создания, обновления, деактивации и удаления ключей
- Каждый ключ может иметь набор claims для контроля доступа
- Централизованная валидация ключей через HTTP API
- Защита административных эндпоинтов ApiKeys с помощью JWT токенов
- Интеграция в другие сервисы с помощью nuget-пакета
- Обновление времени последнего использования ключа

## Структура проекта

- `ApiKeys.Api` - REST API приложение
- `ApiKeys.Logic` - Бизнес-логика сервиса
- `ApiKeys.Dal` - Слой доступа к данным (Entity Framework Core)
- `ApiKeys.Client` - Клиентская библиотека для интеграции в другие сервисы
- `ApiKeys.UnitTests` - Модульные тесты

## Используемые технологии

- **.NET 8.0**
- **ASP.NET Core**
- **Entity Framework Core 8.0**
- **PostgreSQL**
- **JWT Bearer Authentication**
- **Serilog**
- **FluentValidation**

## Конфигурация

Все настройки сервиса берутся из переменных окружения

**Описание переменных:**
- `API_KEYS_CONNECTION_STRING` - строка подключения к PostgreSQL
- `JWT_SECRET` - секретный ключ для подписи JWT токенов
- `ADMIN_USERNAME` - имя пользователя администратора для входа в систему
- `ADMIN_PASSWORD` - пароль администратора
- `ADMIN_CLAIMS` - claims администратора, разделенные точкой с запятой (например: `ManageApiKey;OtherClaim`)

Сервис автоматически загружает переменные из `.env` файла при запуске (через `EnvLoader.LoadEnvFile()`).

## API Документация

Полная документация API доступна в Swagger UI после запуска сервиса:

http://localhost:5215/swagger

## Интеграция в другие сервисы

Для интеграции ApiKeysService в другой сервис (например, TaskService) необходимо:

1. Добавить ссылку на проект `ApiKeys.Client` в ваш сервис

2. Создать DI модуль для аутентификации (например, `AuthDiModule`):

```csharp
using ApiKeys.Client;
using ApiKeys.Client.Extensions;
using DistributedTaskExecutor.Core.DI;

namespace YourService.Api.DI;

public class AuthDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        var apiKeysServiceUrl = configuration["ApiKeysService:BaseUrl"]!;

        services.AddApiKeysClient(apiKeysServiceUrl);
        services.AddApiKeyAuthentication();

        services.AddAuthorization();
    }
}
```

3. Добавить конфигурацию в `appsettings.json`:

```json
{
  "ApiKeysService": {
    "BaseUrl": "http://localhost:5215"
  }
}
```

4. Зарегистрировать модуль в основном DI модуле:

```csharp
public class MainDiModule : IDiModule
{
    private readonly ICollection<IDiModule> modules = new List<IDiModule>
    {
        new LogicDiModule(),
        new AuthDiModule(),  // Добавить модуль аутентификации
        // ... другие модули
    };

    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        foreach (var module in this.modules)
        {
            module.RegisterIn(services, configuration);
        }
    }
}
```

5. Настроить middleware в `Program.cs`:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

6. (Опционально) Настроить Swagger для поддержки API-ключей в `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.AddApiKeySecurity();
});
```

7. Использовать атрибут `[ApiKeyRequired]` в контроллерах:

```csharp
using ApiKeys.Client.Auth;

[ApiController]
[Route("api/example")]
public class YourController : ControllerBase
{
    // Публичный эндпоинт без аутентификации
    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        return this.Ok();
    }

    // Защищенный эндпоинт, требует валидный API-ключ
    [HttpGet("protected")]
    [ApiKeyRequired]
    public IActionResult GetProtected()
    {
        var apiKeyId = this.User.GetApiKeyId();
        var claims = this.User.GetApiKeyClaims();
        
        return this.Ok(new { apiKeyId, claims });
    }

    // Эндпоинт с проверкой конкретного claim
    [HttpGet("admin")]
    [ApiKeyRequired(RequiredClaims = ["admin"])]
    public IActionResult GetAdmin()
    {
        return this.Ok();
    }

    // Эндпоинт с проверкой одного из нескольких claims
    [HttpGet("read-write")]
    [ApiKeyRequired(RequiredClaims = new[] { "read", "write" })]
    public IActionResult GetReadWrite()
    {
        return this.Ok();
    }
}
```

## Использование API-ключей

После интеграции клиенты могут использовать API-ключи для аутентификации двумя способами:

1. **Через заголовок** (по умолчанию `X-ApiKey`):
```
X-ApiKey: your-api-key-here
```

2. **Через query-параметр** (по умолчанию `apiKey`):
```
GET /api/endpoint?apiKey=your-api-key-here
```

