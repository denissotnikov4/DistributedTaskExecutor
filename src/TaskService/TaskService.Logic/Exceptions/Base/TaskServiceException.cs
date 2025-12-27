namespace TaskService.Logic.Exceptions.Base;

public abstract class TaskServiceException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected TaskServiceException(
        string message, 
        int statusCode, 
        string errorCode) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public virtual object ToProblemDetails()
    {
        return new
        {
            Type = $"https://httpstatuses.com/{StatusCode}",
            Title = GetType().Name,
            Status = StatusCode,
            ErrorCode = ErrorCode,
            Detail = Message,
            Instance = Guid.NewGuid().ToString()
        };
    }
}