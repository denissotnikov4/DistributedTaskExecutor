# Распределенный сервис управления задачами с TTL

Распределенный микросервис для постановки, выполнения и мониторинга абстрактных задач с контролем времени жизни процесса (TTL).

## Архитектура

Проект построен на основе **Three-tier Architecture** и состоит из следующих компонентов:

### TaskService (API сервис)
- **TaskService.Api** - REST API для управления задачами
- **TaskService.Logic** - бизнес-логика и сервисы
- **TaskManagement.Dal** - доменные модели и интерфейсы

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
- **Microsoft.Extensions.Logging** - логирование
- **Docker** - контейнеризация

## Функциональность

### Основные возможности:

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
cd TaskService/TaskService.Api
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
    "DefaultConnection": "Host=localhost;Port=5432;Database=TaskServiceDb;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

## API Endpoints

### Получение задачи по ID
```
GET /api/tasks/{id}
Authorization: ApiKey
```

### Получение всех задач
```
GET /api/tasks
Authorization: ApiKey
```

### Повторное выполнение задачи
```
POST /api/tasks/{id}/retry
Authorization: ApiKey
```

### Обновление задачи
```
PATCH /api/tasks/{id}
Authorization: ApiKey
```

## Аутентификация

API защищен ApiKey

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

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

## TTL (Time-to-Live)

TTL задается при создании задачи и определяет максимальное время на выполнение. Если задача не завершена в течение TTL, она автоматически помечается как истекшая и отменяется.

## Масштабирование

Система поддерживает горизонтальное масштабирование:
- Запустите несколько экземпляров API для балансировки нагрузки
- Запустите несколько воркеров для параллельной обработки задач
- RabbitMQ автоматически распределяет задачи между воркерами

## Безопасность

- ApiKey аутентификация
- HTTPS поддержка
- Валидация входных данных
- Защита от SQL инъекций (EF Core)
- Логирование всех операций

## Разработка

### Структура проекта

```
src/
├── TaskService/
│   ├── TaskService.Api/         # API слой
│   ├── TaskManagement.Logic/    # Слой приложения
│   ├── TaskManagement.Dal/      # Слой работы с базой данных
└── WorkerService/
    └── TaskExecutor.Worker/             # Worker сервис
```
