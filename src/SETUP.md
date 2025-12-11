# Инструкция по запуску проекта

## Предварительные требования

1. **.NET 8.0 SDK** - [Скачать](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Docker Desktop** - [Скачать](https://www.docker.com/products/docker-desktop)
3. **PostgreSQL** (опционально, если не используете Docker)
4. **RabbitMQ** (опционально, если не используете Docker)

## Шаг 1: Запуск инфраструктуры

Запустите PostgreSQL и RabbitMQ через Docker Compose:

```bash
docker-compose up -d
```

Проверьте, что контейнеры запущены:

```bash
docker ps
```

Должны быть запущены:
- `taskmanagement-postgres` на порту 5432
- `taskmanagement-rabbitmq` на портах 5672 и 15672

## Шаг 2: Настройка базы данных

Миграции применяются автоматически при запуске API. Если нужно применить вручную:

```bash
cd TaskService/TaskManagement.API
dotnet ef database update --project ../TaskManagement.Infrastructure
```

## Шаг 3: Запуск API сервиса

Откройте терминал в корне проекта:

```bash
cd TaskService/TaskManagement.API
dotnet restore
dotnet run
```

API будет доступен по адресам:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Шаг 4: Запуск Worker сервиса

Откройте новый терминал:

```bash
cd WorkerService/TaskExecutor.Worker
dotnet restore
dotnet run
```

Для масштабирования запустите несколько экземпляров воркера в разных терминалах.

## Шаг 5: Тестирование

### Получение JWT токена

```bash
curl -X POST https://localhost:5001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "test", "password": "test"}'
```

### Создание задачи

```bash
curl -X POST https://localhost:5001/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "description": "Обработать данные",
    "payload": "{\"data\": \"example\"}",
    "ttl": "00:05:00",
    "maxRetries": 3
  }'
```

### Получение задачи по ID

```bash
curl -X GET https://localhost:5001/api/tasks/{TASK_ID} \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Получение всех задач

```bash
curl -X GET https://localhost:5001/api/tasks \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Повторное выполнение задачи

```bash
curl -X POST https://localhost:5001/api/tasks/{TASK_ID}/retry \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Мониторинг

### RabbitMQ Management UI

Откройте в браузере: `http://localhost:15672`

- Логин: `guest`
- Пароль: `guest`

Здесь можно увидеть:
- Очереди задач
- Сообщения в очередях
- Статистику обработки

### Логи

Логи сохраняются в папке `logs/`:
- API: `logs/taskmanagement-{date}.txt`
- Worker: `logs/worker-{date}.txt`

## Docker развертывание

### Сборка образов

```bash
# API
docker build -t taskmanagement-api -f TaskService/TaskManagement.API/Dockerfile .

# Worker
docker build -t taskexecutor-worker -f WorkerService/TaskExecutor.Worker/Dockerfile .
```

### Запуск контейнеров

```bash
# API
docker run -d -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=TaskManagementDb;Username=postgres;Password=postgres" \
  -e RabbitMQ__HostName="host.docker.internal" \
  --name api taskmanagement-api

# Worker (можно запустить несколько экземпляров)
docker run -d \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=TaskManagementDb;Username=postgres;Password=postgres" \
  -e RabbitMQ__HostName="host.docker.internal" \
  --name worker1 taskexecutor-worker

docker run -d \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=TaskManagementDb;Username=postgres;Password=postgres" \
  -e RabbitMQ__HostName="host.docker.internal" \
  --name worker2 taskexecutor-worker
```

## Устранение проблем

### Ошибка подключения к PostgreSQL

Убедитесь, что PostgreSQL запущен:
```bash
docker ps | grep postgres
```

Проверьте строку подключения в `appsettings.json`.

### Ошибка подключения к RabbitMQ

Убедитесь, что RabbitMQ запущен:
```bash
docker ps | grep rabbitmq
```

Проверьте настройки RabbitMQ в `appsettings.json`.

### Проблемы с миграциями

Если миграции не применяются автоматически:

```bash
cd TaskService/TaskManagement.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../TaskManagement.API
dotnet ef database update --startup-project ../TaskManagement.API
```

### Проблемы с сертификатами HTTPS

Для разработки можно отключить HTTPS в `Properties/launchSettings.json` или использовать `--no-https`:

```bash
dotnet run --no-https
```

## Отключение авторизации для тестирования

Для упрощения тестирования можно временно отключить авторизацию:

1. В `TaskService/TaskManagement.API/Program.cs` закомментируйте:
   - `builder.Services.AddAuthentication(...)`
   - `app.UseAuthentication()`
   - `app.UseAuthorization()`

2. В `TaskService/TaskManagement.API/Controllers/TasksController.cs` удалите атрибут `[Authorize]`

## Производительность

Для увеличения производительности:

1. Запустите несколько экземпляров Worker
2. Настройте `prefetchCount` в RabbitMQ (уже настроено на 1)
3. Используйте connection pooling для PostgreSQL
4. Настройте логирование уровня Warning для продакшена

