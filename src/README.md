# Распределенный сервис управления задачами с TTL

Распределенный микросервис для постановки, выполнения и мониторинга абстрактных задач с контролем времени жизни процесса (TTL).

## Архитектура

Проект построен на основе **Clean Architecture** и состоит из следующих компонентов:

### TaskService (API сервис)
- **TaskManagement.Domain** - доменные модели и интерфейсы
- **TaskManagement.Application** - бизнес-логика и сервисы
- **TaskManagement.Infrastructure** - инфраструктура (EF Core, FluentMigrator, RabbitMQ, репозитории)
- **TaskManagement.API** - REST API для управления задачами

### WorkerService (Сервис выполнения задач)
- **TaskExecutor.Domain** - доменные интерфейсы (ICodeExecutor)
- **TaskExecutor.Application** - бизнес-логика обработки задач (ITaskProcessor)
- **TaskExecutor.Infrastructure** - инфраструктура (Roslyn, RabbitMQ, EF Core)
- **TaskExecutor.Worker** - фоновый сервис для выполнения задач из очереди

## Технологии

- **.NET 8.0**
- **ASP.NET Core** - Web API
- **PostgreSQL** - база данных
- **Entity Framework Core** - ORM и миграции
- **RabbitMQ** - очередь сообщений
- **Roslyn** - выполнение C# кода
- **AutoMapper** - маппинг объектов
- **FluentValidation** - валидация
- **JWT Bearer** - аутентификация
- **Serilog** - логирование
- **Docker** - контейнеризация

## Функциональность

### Основные возможности:

1. **Постановка задач**
   - Создание задачи с описанием, данными и TTL
   - Поддержка выполнения реального C# кода
   - Автоматическое добавление в очередь RabbitMQ

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

## Быстрый старт

### Предварительные требования

- .NET 8.0 SDK
- Docker и Docker Compose
- PostgreSQL (или использовать Docker)
- RabbitMQ (или использовать Docker)

### Запуск инфраструктуры

```bash
docker-compose up -d
```

Это запустит:
- PostgreSQL на порту 5432
- RabbitMQ на портах 5672 (AMQP) и 15672 (Management UI)

### Настройка базы данных

Миграции применяются автоматически при запуске приложения.

### Запуск API сервиса

```bash
cd TaskService/TaskManagement.API
dotnet run
```

API будет доступен по адресу: `https://localhost:5001` или `http://localhost:5000`
Swagger UI: `https://localhost:5001/swagger`

### Запуск Worker сервиса

```bash
cd WorkerService/TaskExecutor.Worker
dotnet run
```

Для масштабирования запустите несколько экземпляров воркера.

## Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TaskManagementDb;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "TaskManagement",
    "Audience": "TaskManagement"
  }
}
```

## API Endpoints

### Создание задачи
```
POST /api/tasks
Content-Type: application/json
Authorization: Bearer {token}

{
  "description": "Обработать данные",
  "payload": "{\"data\": \"example\"}",
  "code": "var data = JsonSerializer.Deserialize<Dictionary<string, string>>(Payload); return data[\"data\"].ToUpper();",
  "ttl": "00:05:00",
  "maxRetries": 3
}
```

**Примечание:** Поле `code` опционально. Если указано, WorkerService выполнит этот C# код вместо стандартной обработки payload.

### Получение задачи по ID
```
GET /api/tasks/{id}
Authorization: Bearer {token}
```

### Получение всех задач
```
GET /api/tasks
Authorization: Bearer {token}
```

### Повторное выполнение задачи
```
POST /api/tasks/{id}/retry
Authorization: Bearer {token}
```

## Аутентификация

API защищен JWT Bearer токенами. Для тестирования можно временно отключить авторизацию в `Program.cs`:

```csharp
// Закомментируйте эти строки для тестирования
// builder.Services.AddAuthentication(...)
// app.UseAuthentication();
// app.UseAuthorization();
```

И удалите атрибут `[Authorize]` из контроллера.

## Docker

### Сборка образов

```bash
docker build -t taskmanagement-api -f TaskService/TaskManagement.API/Dockerfile .
docker build -t taskexecutor-worker -f WorkerService/TaskExecutor.Worker/Dockerfile .
```

### Запуск контейнеров

```bash
docker run -d -p 5000:80 --name api taskmanagement-api
docker run -d --name worker1 taskexecutor-worker
docker run -d --name worker2 taskexecutor-worker
```

## Мониторинг

### RabbitMQ Management UI

Доступен по адресу: `http://localhost:15672`
- Логин: `guest`
- Пароль: `guest`

### Логи

Логи сохраняются в папке `logs/`:
- API: `logs/taskmanagement-{date}.txt`
- Worker: `logs/worker-{date}.txt`

## Статусы задач

- **Pending** - задача в очереди, ожидает выполнения
- **InProgress** - задача выполняется воркером
- **Completed** - задача успешно завершена
- **Expired** - время жизни задачи истекло
- **Failed** - задача завершилась с ошибкой
- **Cancelled** - задача отменена

## TTL (Time-to-Live)

TTL задается при создании задачи и определяет максимальное время на выполнение. Если задача не завершена в течение TTL, она автоматически помечается как истекшая и отменяется.

## Масштабирование

Система поддерживает горизонтальное масштабирование:
- Запустите несколько экземпляров API для балансировки нагрузки
- Запустите несколько воркеров для параллельной обработки задач
- RabbitMQ автоматически распределяет задачи между воркерами

## Безопасность

- JWT Bearer аутентификация
- HTTPS поддержка
- Валидация входных данных
- Защита от SQL инъекций (EF Core)
- Логирование всех операций

## Разработка

### Структура проекта

```
src/
├── TaskService/
│   ├── TaskManagement.Domain/          # Доменный слой
│   ├── TaskManagement.Application/      # Слой приложения
│   ├── TaskManagement.Infrastructure/   # Инфраструктурный слой
│   └── TaskManagement.API/              # API слой
└── WorkerService/
    └── TaskExecutor.Worker/             # Worker сервис
```

### Добавление новой функциональности

1. Добавьте доменную модель в `TaskManagement.Domain`
2. Создайте use case в `TaskManagement.Application`
3. Реализуйте репозиторий в `TaskManagement.Infrastructure`
4. Добавьте endpoint в `TaskManagement.API`

## Лицензия

MIT

