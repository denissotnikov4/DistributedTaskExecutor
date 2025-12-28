using Vostok.Configuration.Abstractions.Attributes;
using WorkerService.Cli.Settings.CodeExecution;

namespace WorkerService.Cli.Settings;

public class WorkerServiceSettings
{
    [Required]
    public CodeExecutionSettings CodeExecution { get; init; }

    [Required]
    public RabbitSettings RabbitSettings { get; init; }

    [Required]
    public string ApiKey { get; init; }

    [Required]
    public string TaskServiceApiUrl { get; init; }
}