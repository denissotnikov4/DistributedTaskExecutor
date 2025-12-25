using System.Net;

// ReSharper disable All

namespace TaskService.Client.Api;

public class RequestResult<T> : RequestResultBase
    where T : class
{
    public T? Item { get; }

    private RequestResult(T? item, int statusCode, string? errorMessage = null)
        : base(statusCode, errorMessage)
    {
        this.Item = item;
    }

    public static RequestResult<T> Success(T item, int statusCode)
    {
        return new RequestResult<T>(item, statusCode);
    }

    public static RequestResult<T> Failure(int statusCode, string? errorMessage = null)
    {
        return new RequestResult<T>(null, statusCode, errorMessage);
    }
}

public class RequestResult : RequestResultBase
{
    private RequestResult(int statusCode, string? errorMessage = null)
        : base(statusCode, errorMessage)
    {
    }

    public static RequestResult Success(int statusCode)
    {
        return new RequestResult(statusCode);
    }

    public static RequestResult Failure(int statusCode, string? errorMessage = null)
    {
        return new RequestResult(statusCode, errorMessage);
    }
}

public abstract class RequestResultBase
{
    public int StatusCode { get; }

    public string? ErrorMessage { get; }

    public bool IsSuccess => this.StatusCode is >= 200 and < 300;

    protected RequestResultBase(int statusCode, string? errorMessage = null)
    {
        this.StatusCode = statusCode;
        this.ErrorMessage = errorMessage;
    }

    public void EnsureSuccess()
    {
        if (!this.IsSuccess)
        {
            throw new HttpRequestException(
                $"Request failed with status code {this.StatusCode}: {this.ErrorMessage}.",
                null,
                (HttpStatusCode)this.StatusCode);
        }
    }
}