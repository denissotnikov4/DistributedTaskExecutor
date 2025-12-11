using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using TaskExecutor.Domain.Interfaces;

namespace TaskExecutor.Infrastructure.CodeExecution;

public class RoslynCodeExecutor : ICodeExecutor
{
    private readonly ILogger<RoslynCodeExecutor> _logger;
    private static readonly ScriptOptions DefaultScriptOptions = ScriptOptions.Default
        .WithImports(
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Text",
            "System.Text.Json",
            "System.Threading.Tasks"
        )
        .WithReferences(
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            typeof(JsonSerializer).Assembly
        );

    public RoslynCodeExecutor(ILogger<RoslynCodeExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<object?> ExecuteAsync(string code, string payload, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Compiling and executing C# code");

            // Обертываем код в метод, если это не выражение
            var wrappedCode = WrapCodeIfNeeded(code);

            // Создаем скрипт с глобальными переменными
            var script = CSharpScript.Create<object>(
                wrappedCode,
                options: DefaultScriptOptions,
                globalsType: typeof(ScriptGlobals)
            );

            // Создаем глобальные переменные
            var globals = new ScriptGlobals
            {
                Payload = payload,
                CancellationToken = cancellationToken
            };

            // Компилируем скрипт
            var compilation = script.Compile();
            if (compilation.Any())
            {
                var errors = string.Join("\n", compilation.Select(e => e.ToString()));
                _logger.LogError("Compilation errors: {Errors}", errors);
                throw new InvalidOperationException($"Compilation failed: {errors}");
            }

            // Выполняем скрипт
            var result = await script.RunAsync(globals, cancellationToken);

            _logger.LogInformation("Code executed successfully");
            return result?.ReturnValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing C# code");
            throw;
        }
    }

    private static string WrapCodeIfNeeded(string code)
    {
        // Если код уже является выражением (заканчивается на ;), возвращаем как есть
        if (code.Trim().EndsWith(";"))
        {
            return code;
        }

        // Иначе оборачиваем в return
        return $"return {code};";
    }
}

// Глобальные переменные для скрипта
public class ScriptGlobals
{
    public string Payload { get; set; } = string.Empty;
    public CancellationToken CancellationToken { get; set; }
}

