using Vostok.Configuration.Abstractions.Attributes;

namespace WorkerService.Cli.Settings.CodeExecution;

public class PythonSettings
{
    [Required]
    public string ImageName { get; init; }
}