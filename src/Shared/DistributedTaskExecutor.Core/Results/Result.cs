namespace DistributedTaskExecutor.Core.Results;

public class Result<T>
{
    private readonly Error? error;
    private readonly T? value;

    protected Result(T value)
    {
        this.value = value;
        this.error = null;
    }

    protected Result(Error error)
    {
        this.error = error;
        this.value = default;
    }

    public T Value
    {
        get
        {
            if (this.IsFailure)
                throw new InvalidOperationException(
                    "Cannot access Value when result is a failure. Check IsFailure first.");
            return this.value!;
        }
    }

    public Error Error
    {
        get
        {
            if (!this.IsFailure)
                throw new InvalidOperationException(
                    "Cannot access Error when result is a success. Check IsFailure first.");
            return this.error!;
        }
    }

    public bool IsFailure => this.error != null;
    public bool IsSuccess => !this.IsFailure;

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    public static implicit operator Result<T>(Error error)
    {
        return new Result<T>(error);
    }
}

public class Result
{
    private readonly Error? error;

    protected Result()
    {
        this.error = null;
    }

    protected Result(Error error)
    {
        this.error = error;
    }

    public Error Error
    {
        get
        {
            if (!this.IsFailure)
                throw new InvalidOperationException(
                    "Cannot access Error when result is a success. Check IsFailure first.");
            return this.error!;
        }
    }

    public bool IsFailure => this.error != null;
    public bool IsSuccess => !this.IsFailure;

    public static Result Success => new();

    public static implicit operator Result(Error error)
    {
        return new Result(error);
    }
}