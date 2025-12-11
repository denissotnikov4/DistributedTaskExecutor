namespace TaskExecutor.Domain.Interfaces;

public interface ICodeExecutor
{
    Task<object?> ExecuteAsync(string code, string payload, CancellationToken cancellationToken = default);
}

