# Выполнение C# кода в WorkerService

## Обзор

WorkerService поддерживает выполнение реального C# кода, который клиент отправляет через API. Это реализовано с использованием **Roslyn** (Microsoft.CodeAnalysis.CSharp.Scripting).

## Варианты выполнения кода

### 1. Roslyn Scripting (Текущая реализация)

**Преимущества:**
- Выполнение кода во время выполнения
- Не требует компиляции в отдельные сборки
- Поддержка выражений и простых скриптов
- Изоляция через AppDomain (частичная)

**Ограничения:**
- Не поддерживает полные классы и методы
- Ограниченный набор доступных библиотек
- Производительность ниже, чем у скомпилированного кода

### 2. Компиляция в динамические сборки (Альтернатива)

Можно реализовать компиляцию C# кода в динамические сборки:

```csharp
// Пример реализации
var compilation = CSharpCompilation.Create(
    "DynamicAssembly",
    syntaxTrees: new[] { CSharpSyntaxTree.ParseText(code) },
    references: GetReferences(),
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
);
```

**Преимущества:**
- Поддержка полных классов и методов
- Лучшая производительность
- Больше возможностей

**Недостатки:**
- Сложнее в реализации
- Требует управления сборками в памяти

### 3. Изолированное выполнение через Docker (Рекомендуется для продакшена)

Для безопасности можно выполнять код в изолированных Docker контейнерах:

```csharp
// Пример
var container = await dockerClient.Containers.CreateContainerAsync(
    new CreateContainerParameters
    {
        Image = "dotnet-sdk",
        Cmd = new[] { "dotnet", "run", "--code", code }
    }
);
```

**Преимущества:**
- Полная изоляция
- Безопасность
- Можно ограничить ресурсы

**Недостатки:**
- Требует Docker
- Медленнее из-за создания контейнеров

## Текущая реализация

### Использование

При создании задачи через API можно передать C# код:

```json
{
  "description": "Вычислить сумму",
  "payload": "{\"a\": 5, \"b\": 3}",
  "code": "var data = JsonSerializer.Deserialize<Dictionary<string, int>>(Payload); return data[\"a\"] + data[\"b\"];",
  "ttl": "00:05:00",
  "maxRetries": 3
}
```

### Доступные переменные в коде

- `Payload` (string) - JSON строка с данными задачи
- `CancellationToken` - Токен отмены для контроля TTL

### Примеры кода

#### Простое вычисление

```csharp
var data = JsonSerializer.Deserialize<Dictionary<string, int>>(Payload);
return data["a"] * data["b"];
```

#### Обработка данных

```csharp
var data = JsonSerializer.Deserialize<Dictionary<string, object>>(Payload);
var result = new Dictionary<string, object>
{
    ["processed"] = true,
    ["timestamp"] = DateTime.UtcNow,
    ["data"] = data
};
return result;
```

#### Асинхронная операция

```csharp
await Task.Delay(1000, CancellationToken);
var data = JsonSerializer.Deserialize<Dictionary<string, string>>(Payload);
return $"Processed: {data["message"]}";
```

## Безопасность

### Текущие ограничения

1. **Sandboxing:** Частичная изоляция через Roslyn
2. **Доступные библиотеки:** Ограниченный набор
3. **Время выполнения:** Контролируется через TTL

### Рекомендации для продакшена

1. **Валидация кода:**
   - Проверка на опасные операции (File.IO, Network, etc.)
   - Ограничение использования определенных типов

2. **Изоляция:**
   - Использование Docker контейнеров
   - Ограничение ресурсов (CPU, память)

3. **Мониторинг:**
   - Логирование всех выполняемых операций
   - Отслеживание использования ресурсов

## Расширение функциональности

### Добавление новых библиотек

В `RoslynCodeExecutor.cs` можно добавить дополнительные ссылки:

```csharp
.WithReferences(
    typeof(object).Assembly,
    typeof(Enumerable).Assembly,
    typeof(JsonSerializer).Assembly,
    typeof(YourCustomType).Assembly  // Добавить свою библиотеку
)
```

### Добавление новых глобальных переменных

Расширьте класс `ScriptGlobals`:

```csharp
public class ScriptGlobals
{
    public string Payload { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public ILogger Logger { get; set; }  // Добавить логгер
    public HttpClient HttpClient { get; set; }  // Добавить HTTP клиент
}
```

## Альтернативные подходы

### 1. Предопределенные функции

Вместо выполнения произвольного кода, можно использовать предопределенные функции:

```csharp
public enum TaskFunction
{
    CalculateSum,
    ProcessData,
    TransformJson
}
```

### 2. Плагинная архитектура

Загружать плагины из DLL файлов:

```csharp
var assembly = Assembly.LoadFrom("plugin.dll");
var plugin = assembly.CreateInstance("PluginClass");
var result = plugin.Execute(payload);
```

### 3. Scripting языки

Использовать специализированные языки (Lua, Python через IronPython):

```csharp
// Пример с Lua
var lua = new Lua();
lua["payload"] = payload;
var result = lua.DoString(code);
```

## Производительность

### Оптимизации

1. **Кэширование скомпилированных скриптов:**
   ```csharp
   private readonly Dictionary<string, Script<object>> _scriptCache = new();
   ```

2. **Предкомпиляция:**
   - Компилировать код один раз
   - Переиспользовать для похожих задач

3. **Пул потоков:**
   - Использовать ThreadPool для выполнения
   - Ограничить количество одновременных выполнений

## Примеры использования

### Пример 1: Математические вычисления

```json
{
  "description": "Вычислить факториал",
  "payload": "{\"n\": 5}",
  "code": "var n = JsonSerializer.Deserialize<Dictionary<string, int>>(Payload)[\"n\"]; int fact = 1; for (int i = 1; i <= n; i++) fact *= i; return fact;",
  "ttl": "00:01:00"
}
```

### Пример 2: Обработка строк

```json
{
  "description": "Обработать строку",
  "payload": "{\"text\": \"Hello World\"}",
  "code": "var data = JsonSerializer.Deserialize<Dictionary<string, string>>(Payload); return data[\"text\"].ToUpper().Reverse();",
  "ttl": "00:01:00"
}
```

### Пример 3: Работа с коллекциями

```json
{
  "description": "Фильтрация данных",
  "payload": "{\"numbers\": [1,2,3,4,5,6,7,8,9,10]}",
  "code": "var data = JsonSerializer.Deserialize<Dictionary<string, int[]>>(Payload); return data[\"numbers\"].Where(n => n % 2 == 0).ToArray();",
  "ttl": "00:01:00"
}
```

