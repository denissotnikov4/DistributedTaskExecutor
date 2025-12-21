namespace WorkerService.Cli.Exceptions;

public class DotnetProjectInitException : Exception
{
    public DotnetProjectInitException(string message)
        : base($"Dotnet project initialization error: Message ->  {message}.")
    {
    }
}