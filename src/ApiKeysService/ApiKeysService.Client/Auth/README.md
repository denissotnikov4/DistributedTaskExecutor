# Интеграция аутентификации по API-ключам

Этот модуль предоставляет готовую инфраструктуру для интеграции аутентификации по API-ключам в другие сервисы.

## Быстрый старт

### 1. Добавьте зависимость на ApiKeysService.Client

```xml

<ItemGroup>
    <ProjectReference Include="..\ApiKeysService.Client\ApiKeysService.Client.csproj"/>
</ItemGroup>
```

### 2. Зарегистрируйте клиент ApiKeysService

В `Program.cs` или DI модуле:

```csharp
using ApiKeysService.Client;
using ApiKeysService.Client.Authentication;

// Регистрация клиента
builder.Services.AddApiKeysClient("http://apikeys-service:5000");

// Регистрация аутентификации
builder.Services.AddApiKeyAuthentication(options =>
{
    options.HeaderName = "X-Api-Key"; // По умолчанию
    options.QueryParameterName = "apiKey"; // Опционально
});
```

### 3. Настройте pipeline в Program.cs

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### 4. Используйте в контроллерах

#### Вариант 1: Атрибут на уровне контроллера

```csharp
[ApiController]
[Route("api/tasks")]
[ApiKeyRequired] // Все действия требуют API-ключ
public class TasksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetTasks()
    {
        var apiKeyId = User.GetApiKeyId();
        var claims = User.GetApiKeyClaims();
        
        // Ваша логика
        return Ok();
    }
}
```

#### Вариант 2: Атрибут на уровне действия

```csharp
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    [HttpGet]
    [ApiKeyRequired] // Только это действие требует API-ключ
    public IActionResult GetTasks()
    {
        return Ok();
    }
    
    [HttpPost]
    // Это действие доступно без API-ключа
    public IActionResult CreateTask()
    {
        return Ok();
    }
}
```

#### Вариант 3: Проверка claims через атрибут

```csharp
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    [HttpGet]
    // Требует наличие хотя бы одного из указанных claims
    [ApiKeyRequired(RequiredClaims = new[] { "tasks.read", "admin" })]
    public IActionResult GetTasks()
    {
        return Ok(GetAllTasks());
    }
    
    [HttpPost]
    // Требует конкретный claim
    [ApiKeyRequired(RequiredClaims = new[] { "tasks.write" })]
    public IActionResult CreateTask()
    {
        return Ok();
    }
    
    [HttpDelete("{id}")]
    // Требует только валидный API-ключ, без проверки claims
    [ApiKeyRequired]
    public IActionResult DeleteTask(Guid id)
    {
        // Проверка claims в коде, если нужно
        if (User.HasApiKeyClaim("admin"))
        {
            return Ok();
        }
        
        return Forbid();
    }
}
```

#### Вариант 4: Проверка claims в коде

```csharp
[ApiController]
[Route("api/tasks")]
[ApiKeyRequired]
public class TasksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetTasks()
    {
        // Проверка наличия конкретного claim
        if (User.HasApiKeyClaim("admin"))
        {
            // Логика для администратора
            return Ok(GetAllTasks());
        }
        
        // Проверка наличия хотя бы одного из claims
        if (User.HasAnyApiKeyClaim(new[] { "tasks.read", "tasks.write" }))
        {
            return Ok(GetUserTasks());
        }
        
        return Forbid();
    }
}
```

## Настройка опций

```csharp
builder.Services.AddApiKeyAuthentication(options =>
{
    // Название схемы аутентификации
    options.SchemeName = "ApiKey";
    
    // Название заголовка с API-ключом
    options.HeaderName = "X-Api-Key";
    
    // Название query параметра (null = не использовать)
    options.QueryParameterName = "apiKey";
    
    // Название claim типа для ID ключа
    options.ApiKeyIdClaimType = "ApiKeyId";
    
    // Название claim типа для claims из ключа
    options.ApiKeyClaimType = "ApiKeyClaim";
});
```

## Использование в коде

### Проверка claims через атрибут (рекомендуется)

```csharp
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    // Требует наличие хотя бы одного из claims: "tasks.read" или "admin"
    [HttpGet]
    [ApiKeyRequired(RequiredClaims = new[] { "tasks.read", "admin" })]
    public IActionResult GetTasks()
    {
        return Ok();
    }
}
```

### Получение информации о ключе в коде

```csharp
public class MyController : ControllerBase
{
    [HttpGet]
    [ApiKeyRequired]
    public IActionResult MyAction()
    {
        // Получить ID API-ключа
        var apiKeyId = User.GetApiKeyId();
        
        // Получить все claims из ключа
        var claims = User.GetApiKeyClaims();
        
        // Проверить наличие конкретного claim
        if (User.HasApiKeyClaim("admin"))
        {
            // Логика для администратора
        }
        
        // Проверить наличие хотя бы одного из claims
        if (User.HasAnyApiKeyClaim(new[] { "read", "write" }))
        {
            // Логика
        }
        
        return Ok();
    }
}
```

## Интеграция с существующей аутентификацией

Если у вас уже есть JWT аутентификация, можно использовать обе схемы:

```csharp
builder.Services.AddAuthentication()
    .AddJwtBearer("JWT", options => { /* ... */ })
    .AddApiKey("ApiKey", options => { /* ... */ });

// В контроллере можно указать обе схемы
[Authorize(AuthenticationSchemes = "JWT,ApiKey")]
public class MyController : ControllerBase
{
    // ...
}
```

## Обработка ошибок

При невалидном API-ключе будет возвращен статус 401 Unauthorized с сообщением об ошибке.

## Пример полной интеграции

```csharp
// Program.cs
using ApiKeysService.Client;
using ApiKeysService.Client.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Регистрация клиента
builder.Services.AddApiKeysClient(
    builder.Configuration["ApiKeysService:BaseUrl"] ?? "http://localhost:5000");

// Регистрация аутентификации
builder.Services.AddApiKeyAuthentication(options =>
{
    options.HeaderName = "X-Api-Key";
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

```csharp
// Controllers/TasksController.cs
using ApiKeysService.Client.Authentication;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/tasks")]
[ApiKeyRequired]
public class TasksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetTasks()
    {
        var apiKeyId = User.GetApiKeyId();
        // Ваша логика
        return Ok();
    }
}
```

