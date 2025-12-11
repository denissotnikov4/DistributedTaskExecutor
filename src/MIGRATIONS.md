# Работа с миграциями базы данных

## Создание новой миграции

```bash
cd TaskService/TaskManagement.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../TaskManagement.API
```

## Применение миграций

### Автоматически (при запуске API)

Миграции применяются автоматически при запуске API сервиса.

### Вручную

```bash
cd TaskService/TaskManagement.API
dotnet ef database update --project ../TaskManagement.Infrastructure
```

## Откат миграции

```bash
cd TaskService/TaskManagement.API
dotnet ef database update PreviousMigrationName --project ../TaskManagement.Infrastructure
```

## Удаление последней миграции

```bash
cd TaskService/TaskManagement.Infrastructure
dotnet ef migrations remove --startup-project ../TaskManagement.API
```

## Просмотр списка миграций

```bash
cd TaskService/TaskManagement.API
dotnet ef migrations list --project ../TaskManagement.Infrastructure
```

## Установка EF Core Tools (если не установлены)

```bash
dotnet tool install --global dotnet-ef
```

## Структура базы данных

### Таблица Tasks

- `Id` (uuid, PK) - Уникальный идентификатор задачи
- `Description` (varchar(500)) - Описание задачи
- `Payload` (text) - JSON данные для обработки
- `Status` (integer) - Статус задачи (0-5)
- `CreatedAt` (timestamp) - Время создания
- `StartedAt` (timestamp, nullable) - Время начала выполнения
- `CompletedAt` (timestamp, nullable) - Время завершения
- `ExpiresAt` (timestamp, nullable) - Время истечения TTL
- `Ttl` (interval) - Максимальное время на выполнение
- `Result` (text, nullable) - JSON результат выполнения
- `ErrorMessage` (text, nullable) - Сообщение об ошибке
- `WorkerId` (text, nullable) - ID воркера, выполняющего задачу
- `RetryCount` (integer, default: 0) - Количество попыток
- `MaxRetries` (integer, default: 3) - Максимальное количество попыток
- `Code` (text, nullable) - C# код для выполнения (опционально)

### Индексы

- `IX_Tasks_Status` - Индекс по статусу
- `IX_Tasks_ExpiresAt` - Индекс по времени истечения

