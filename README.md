# Распределенный сервис управления задачами с TTL

Распределенная система для постановки, выполнения и мониторинга абстрактных задач с контролем времени жизни процесса (TTL).

## Основные возможности

1. **Постановка задач**
   - Создание задачи с описанием, данными и TTL
   - Поддержка выполнения реального C#, Python кода
   - Добавление в очередь RabbitMQ

2. **Выполнение задач**
   - Распределенное выполнение между несколькими воркерами
   - Контроль TTL с автоматической отменой при истечении времени
   - Обработка ошибок и повторные попытки

3. **Мониторинг**
   - Получение статуса задачи по ID
   - Просмотр всех задач
   - Отслеживание состояния (Pending, InProgress, Completed, Expired, Failed)

4. **Повторное выполнение**
   - Возможность повторного запуска неудачных или истекших задач

## Архитектура

Система состоит из 4-х основных компонентов:

### 1. TaskService

<details>
<summary>
   Описание сервиса
</summary>

Центральное АПИ системы: работа с задачами, мониторинг, контроль TTL, авторизация.

#### Структура проекта

- `TaskService.Api` - REST API приложение
- `TaskService.Logic` - Бизнес-логика
- `TaskService.Dal` - Доступ к данным (EF Core)
- `TaskService.Client` - Клиентская библиотека для интеграции в другие сервисы
- `TaskService.Tests` - Тесты

#### Технологии

- **.NET 8.0**
- **ASP.NET Core** - Web API
- **PostgreSQL** - база данных
- **Entity Framework Core** - ORM и миграции
- **RabbitMQ** - очередь сообщений
- **AutoMapper** - маппинг объектов
- **FluentValidation** - валидация
- **Microsoft.Extensions.Logging** - логирование
- **Docker** - контейнеризация

#### API Endpoints

##### Создание задачи
```
POST /api/tasks
Authorization: ApiKey
```

##### Получение задачи по ID
```
GET /api/tasks/{id}
Authorization: ApiKey
```

##### Получение всех задач
```
GET /api/tasks
Authorization: ApiKey
```

##### Повторное выполнение задачи
```
POST /api/tasks/{id}/retry
Authorization: ApiKey
```

##### Обновление задачи
```
PATCH /api/tasks/{id}
Authorization: ApiKey
```

#### Статусы задач

- **Pending** - задача в очереди, ожидает выполнения
- **InProgress** - задача выполняется воркером
- **Completed** - задача успешно завершена
- **Expired** - время жизни задачи истекло
- **Failed** - задача завершилась с ошибкой

#### TTL (Time-to-Live)

TTL задается при создании задачи и определяет максимальное время на выполнение. Если задача не завершена в течение TTL, она автоматически помечается как истекшая и отменяется.

Контроль TTL происходит с помощью фонового процесса, отслеживающего активное время задач. Если время, выделенное на задачу, прошло, то процесс переводит последнюю в статус `Expired`.

#### Конфигурация (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TaskServiceDb;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

#### Безопасность

- ApiKey аутентификация
- HTTPS поддержка
- Валидация входных данных
- Защита от SQL инъекций (EF Core)
- Логирование всех операций

</details>

### 2. ApiKeysService
<details>
<summary>
Описание сервиса
</summary>

Сервис для управления API-ключами в распределенной системе. Предоставляет функциональность создания, управления и валидации API-ключей с поддержкой claims для авторизации.

#### Структура проекта

- `ApiKeys.Api` - REST API приложение
- `ApiKeys.Logic` - Бизнес-логика сервиса
- `ApiKeys.Dal` - Слой доступа к данным (Entity Framework Core)
- `ApiKeys.Client` - Клиентская библиотека для интеграции в другие сервисы
- `ApiKeys.UnitTests` - Модульные тесты

#### Используемые технологии

- **.NET 8.0**
- **ASP.NET Core**
- **Entity Framework Core 8.0**
- **PostgreSQL**
- **JWT Bearer Authentication**
- **Serilog**
- **FluentValidation**

#### Особенности

- API-ключи хранятся в виде хэшей, оригинальные ключи возвращаются только при создании
- Поддержка создания, обновления, деактивации и удаления ключей
- Каждый ключ может иметь набор claims для контроля доступа
- Централизованная валидация ключей через HTTP API
- Защита административных эндпоинтов ApiKeys с помощью JWT токенов
- Интеграция в другие сервисы с помощью nuget-пакета
- Обновление времени последнего использования ключа

#### Конфигурация

Все настройки сервиса берутся из переменных окружения

**Описание переменных:**
- `API_KEYS_CONNECTION_STRING` - строка подключения к PostgreSQL
- `JWT_SECRET` - секретный ключ для подписи JWT токенов
- `ADMIN_USERNAME` - имя пользователя администратора для входа в систему
- `ADMIN_PASSWORD` - пароль администратора
- `ADMIN_CLAIMS` - claims администратора, разделенные точкой с запятой (например: `ManageApiKey;OtherClaim`)

Сервис автоматически загружает переменные из `.env` файла при запуске (через `EnvLoader.LoadEnvFile()`).

#### API Документация

Полная документация API доступна в Swagger UI после запуска сервиса:

http://localhost:5215/swagger

#### Интеграция в другие сервисы

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

#### Использование API-ключей

После интеграции клиенты могут использовать API-ключи для аутентификации двумя способами:

1. **Через заголовок** (по умолчанию `X-ApiKey`):
```
X-ApiKey: your-api-key-here
```

2. **Через query-параметр** (по умолчанию `apiKey`):
```
GET /api/endpoint?apiKey=your-api-key-here
```
</details>

### 3. WorkerService
<details>
<summary>
     Описание сервиса
</summary>

Сервис непосредственного выполнения абстрактных задач.

#### Структура проекта

- `WorkerService.Cli` - консольное приложение
- `WorkerService.Tests` - тесты для сервиса

#### Используемые технологии

- **.NET 8.0 / dotnet CLI**
- **Docker / Docker CLI**
- **Serilog**
- **FluentValidation**
- **RabbitMQ**

#### Поддерживаемые языки программирования для задач

- **CSharp**
- **Python**

#### Особенности

- Сервис запускает код каждой задачи в отдельном docker-контейнере с минимальными правами для обеспечения безопасности хоста
- Сервис позволяет конфигурировать версии dotnet / python, а также различные параметры выполнения кода через файл `settings.json` и переменные окружения

#### Настройки (settings.json)

```json
{
  "CodeExecution": {
    "Timeout": "00:05:00",
    "Docker": {
      "CpuLimit": 0.25,
      "MemoryMbLimit": 256,
      "PidLimit": 10
    },
    "CSharp": {
      "DotnetSdkImageName": "mcr.microsoft.com/dotnet/sdk:8.0",
      "DotnetRuntimeImageName": "mcr.microsoft.com/dotnet/runtime:8.0",
      "FrameworkVersion": "net8.0",
      "LanguageVersion": "12.0"
    },
    "Python": {
      "ImageName": "python:3.11-slim"
    },
    "Rabbit": {
      "AppName": "WorkerService",
      "HostName": "localhost",
      "UserName": "guest",
      "Password": "guest",
      "Port": 5672,
      "QueueName": "task_queue",
      "ExchangeName": "task_exchange",
      "ExchangeType": "direct",
      "RoutingKey": "task_key"
    },
    "TaskServiceApiUrl": "http://task-service"
  }
}
```

#### Алгоритм работы
При получении сообщения (ид задачи) из очереди сервис:
1. Получает из основного АПИ полное представление задачи через библиотеку клиента
2. Создает в директории `/tmp` директорию для текущей задачи
3. В созданную директорию кладет пользовательский код и Dockerfile, соответствующий языку программирования задачи
4. С помощью `Docker CLI` и класса `Process` идет запуск заранее написанного шаблона `Dockerfile`, выполняющего предварительную сборку (если необходимо) и запуск кода
5. Результат выполнения получается через `stdout` / `stderr` и записывается в модель `Task`
6. Текущее сообщение из `RabbitMQ` отмечается как `acked`

#### Запуск в Docker

```
docker run -it \
    -v /var/run/docker.sock:/var/run/docker.sock \
    -v <path-to-local-settings.json>:/app/settings.json \
    -e "ApiKey=my-api-key" \
    -e "Rabbit__UserName=my-user-name" \
    -e "Rabbit__Password=my-password" \
    worker-service:latest
```

* Все обязательные настройки кроме секретов указаны в `settings.json`

</details>

### 4. Shared

<details>
<summary>
   Описание проекта
</summary>

Решение с проектами, код которых используется в разных местах системы.

#### Текущие проекты
- `DistributedTaskExecutor.Core` - общие методы для работы с `RabbitMQ` / базами данных / авторизацией, а также множество helper-классов

</details>

## Взаимодействие между компонентами

#### Успешное создание задачи
<img width="1099" height="668" alt="изображение" src="https://github.com/user-attachments/assets/09e446d0-bb8b-4638-8188-c5dc5e4a34d1" />

#### Успешная обработка задачи WorkerService
<img width="885" height="755" alt="изображение" src="https://github.com/user-attachments/assets/14fc0cf6-0091-4f5a-a6b3-6611ebd5f467" />

* TaskService.Api и ApiKeysService.Api имеют клиентов для предоставления удобного доступа к своему функционалу

* Клиенты публикуются в виде nuget-пакетов через настроенный github-action и используются в других компонентах системы (клиент TaskService.Api - в WorkerService, клиент ApiKeysService.Api в TaskService.Api)

* Библиотека `Core` также публикуется nuget-пакетом и используется во всех компонентах системы

## Масштабирование

Система поддерживает горизонтальное масштабирование:
- Запустите несколько экземпляров API для балансировки нагрузки
- Запустите несколько воркеров для параллельной обработки задач
