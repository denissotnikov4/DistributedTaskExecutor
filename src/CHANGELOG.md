# Changelog

## [2.1.0] - 2024-01-01

### Изменения

#### Миграции базы данных
- ✅ **Вернул EF Core Migrations**
  - Убрал FluentMigrator
  - Использую стандартные EF Core Migrations
  - Автоматическое применение при запуске

## [2.0.0] - 2024-01-01

### Изменения

#### Архитектура WorkerService
- ✅ **Реорганизован на Clean Architecture**
  - `TaskExecutor.Domain` - доменные интерфейсы
  - `TaskExecutor.Application` - бизнес-логика
  - `TaskExecutor.Infrastructure` - инфраструктура (Roslyn, RabbitMQ, EF)
  - `TaskExecutor.Worker` - точка входа

#### Удаление MediatR
- ✅ **Убрал MediatR из Application слоя**
  - Заменен на прямые сервисы (`ITaskService`)
  - Упрощена архитектура
  - Меньше зависимостей

#### Выполнение C# кода
- ✅ **Добавлена поддержка выполнения реального C# кода**
  - Реализация через Roslyn (Microsoft.CodeAnalysis.CSharp.Scripting)
  - Клиент может отправлять C# код через API
  - Код выполняется в WorkerService с контролем TTL
  - Поддержка глобальных переменных (Payload, CancellationToken)

### Новые возможности

1. **Выполнение C# кода:**
   ```json
   {
     "description": "Вычислить сумму",
     "payload": "{\"a\": 5, \"b\": 3}",
     "code": "var data = JsonSerializer.Deserialize<Dictionary<string, int>>(Payload); return data[\"a\"] + data[\"b\"];",
     "ttl": "00:05:00"
   }
   ```

2. **FluentMigrator:**
   - Более гибкие миграции
   - Лучший контроль версий

3. **Clean Architecture в WorkerService:**
   - Разделение ответственности
   - Легче тестировать
   - Проще расширять

### Удаленные компоненты

- MediatR и все связанные команды/запросы
- EF Core Migrations (заменены на FluentMigrator)
- Старая структура WorkerService

### Миграция с версии 1.0

1. **Обновите миграции:**
   ```bash
   # Старые миграции EF Core будут удалены
   # FluentMigrator применит миграции автоматически при запуске
   ```

2. **Обновите API вызовы:**
   - MediatR больше не используется
   - API остался прежним, внутренняя реализация изменилась

3. **Добавьте поле Code в задачи:**
   - Новое поле `Code` в таблице `Tasks`
   - Миграция применится автоматически

### Breaking Changes

- ❌ Удален MediatR - используйте `ITaskService` напрямую
- ❌ Изменена структура миграций - используйте FluentMigrator
- ✅ API остался совместимым

### Документация

- `CODE_EXECUTION.md` - подробное описание выполнения C# кода
- `ARCHITECTURE.md` - обновлена архитектура WorkerService

