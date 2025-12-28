namespace DistributedTaskExecutor.Core.Results;

public class Error
{

    protected Error(string message)
    {
        this.Message = message;
    }
    public string Message { get; }

    public static Error Failure(string message = "")
    {
        return new Error(message);
    }
}