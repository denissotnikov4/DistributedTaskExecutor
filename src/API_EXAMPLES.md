# Примеры использования API

## Аутентификация

### Получение JWT токена

```bash
POST /api/auth/token
Content-Type: application/json

{
  "username": "test",
  "password": "test"
}
```

**Ответ:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2024-01-02T12:00:00Z"
}
```

## Управление задачами

### Создание задачи

```bash
POST /api/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "description": "Обработать данные пользователя",
  "payload": "{\"userId\": 123, \"action\": \"process\"}",
  "ttl": "00:05:00",
  "maxRetries": 3
}
```

**Ответ (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Обработать данные пользователя",
  "payload": "{\"userId\": 123, \"action\": \"process\"}",
  "status": 0,
  "createdAt": "2024-01-01T12:00:00Z",
  "startedAt": null,
  "completedAt": null,
  "expiresAt": "2024-01-01T12:05:00Z",
  "ttl": "00:05:00",
  "result": null,
  "errorMessage": null,
  "workerId": null,
  "retryCount": 0,
  "maxRetries": 3
}
```

### Получение задачи по ID

```bash
GET /api/tasks/{id}
Authorization: Bearer {token}
```

**Ответ (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Обработать данные пользователя",
  "payload": "{\"userId\": 123, \"action\": \"process\"}",
  "status": 1,
  "createdAt": "2024-01-01T12:00:00Z",
  "startedAt": "2024-01-01T12:00:05Z",
  "completedAt": null,
  "expiresAt": "2024-01-01T12:05:00Z",
  "ttl": "00:05:00",
  "result": null,
  "errorMessage": null,
  "workerId": "WORKER-12345",
  "retryCount": 0,
  "maxRetries": 3
}
```

### Получение всех задач

```bash
GET /api/tasks
Authorization: Bearer {token}
```

**Ответ (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "description": "Обработать данные пользователя",
    "status": 2,
    "createdAt": "2024-01-01T12:00:00Z",
    "completedAt": "2024-01-01T12:00:10Z",
    "result": "{\"processed\": true, \"workerId\": \"WORKER-12345\"}"
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "description": "Другая задача",
    "status": 3,
    "createdAt": "2024-01-01T11:00:00Z",
    "completedAt": "2024-01-01T11:06:00Z",
    "errorMessage": "Task expired due to TTL"
  }
]
```

### Повторное выполнение задачи

```bash
POST /api/tasks/{id}/retry
Authorization: Bearer {token}
```

**Ответ (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Обработать данные пользователя",
  "status": 0,
  "retryCount": 1,
  "expiresAt": "2024-01-01T12:10:00Z"
}
```

## Статусы задач

- `0` - Pending (В ожидании)
- `1` - InProgress (В процессе выполнения)
- `2` - Completed (Завершена успешно)
- `3` - Expired (Истекла)
- `4` - Failed (Завершена с ошибкой)
- `5` - Cancelled (Отменена)

## Формат TTL

TTL задается в формате `TimeSpan`:
- `"00:01:00"` - 1 минута
- `"00:05:00"` - 5 минут
- `"01:00:00"` - 1 час
- `"24:00:00"` - 24 часа (максимум)

## Примеры использования с curl

### Создание задачи

```bash
TOKEN=$(curl -s -X POST https://localhost:5001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "test", "password": "test"}' | jq -r '.token')

curl -X POST https://localhost:5001/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "description": "Тестовая задача",
    "payload": "{\"test\": \"data\"}",
    "ttl": "00:02:00",
    "maxRetries": 2
  }'
```

### Мониторинг задачи

```bash
TASK_ID="3fa85f64-5717-4562-b3fc-2c963f66afa6"

# Проверка статуса
curl -X GET https://localhost:5001/api/tasks/$TASK_ID \
  -H "Authorization: Bearer $TOKEN"

# Повторное выполнение при ошибке
curl -X POST https://localhost:5001/api/tasks/$TASK_ID/retry \
  -H "Authorization: Bearer $TOKEN"
```

## Примеры использования с PowerShell

### Создание задачи

```powershell
$tokenResponse = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/token" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"username": "test", "password": "test"}'

$token = $tokenResponse.token

$task = @{
    description = "Тестовая задача"
    payload = '{"test": "data"}'
    ttl = "00:02:00"
    maxRetries = 2
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/tasks" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{Authorization = "Bearer $token"} `
  -Body $task
```

### Получение задачи

```powershell
$taskId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"

Invoke-RestMethod -Uri "https://localhost:5001/api/tasks/$taskId" `
  -Method Get `
  -Headers @{Authorization = "Bearer $token"}
```

## Обработка ошибок

### Задача не найдена (404)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

### Неверный запрос (400)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "Description": ["Description is required"],
    "Ttl": ["TTL must be greater than zero"]
  }
}
```

### Неавторизованный доступ (401)

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

