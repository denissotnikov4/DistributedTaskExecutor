namespace WorkerService.Cli.Exceptions;

public class DockerBuildException : Exception
{
    public DockerBuildException(string imageName, string error)
        : base($"Docker build error: Image -> \"{imageName}\", Error -> {error}.")
    {
    }
}