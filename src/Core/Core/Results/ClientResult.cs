namespace Core.Results;

/// <summary>
/// Результат выполнения операции HTTP-клиента
/// </summary>
public class ClientResult<T> : Result<T>
{
    private ClientResult(T value) : base(value)
    {
    }

    private ClientResult(Error error) : base(error)
    {
    }

    public new static ClientResult<T> Success(T value) => new(value);

    public new static ClientResult<T> Failure(Error error) => new(error);

    public static ClientResult<T> Failure(ClientError error) => new(error);

    public static implicit operator ClientResult<T>(T value)
    {
        return new ClientResult<T>(value);
    }

    public static implicit operator ClientResult<T>(Error error)
    {
        return new ClientResult<T>(error);
    }

    public static implicit operator ClientResult<T>(ClientError error)
    {
        return new ClientResult<T>(error);
    }
}

/// <summary>
/// Результат выполнения операции HTTP-клиента без возвращаемого значения
/// </summary>
public class ClientResult : Result
{
    private ClientResult()
    {
    }

    private ClientResult(Error error) : base(error)
    {
    }

    public new static ClientResult Success => new();

    public new static ClientResult Failure(Error error) => new(error);

    public static ClientResult Failure(ClientError error) => new(error);

    public static implicit operator ClientResult(Error error)
    {
        return new ClientResult(error);
    }

    public static implicit operator ClientResult(ClientError error)
    {
        return new ClientResult(error);
    }
}