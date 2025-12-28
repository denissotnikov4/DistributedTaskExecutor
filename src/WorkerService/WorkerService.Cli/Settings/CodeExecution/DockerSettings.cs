using Vostok.Configuration.Abstractions.Attributes;

namespace WorkerService.Cli.Settings.CodeExecution;

public class DockerSettings
{
    [Required]
    public double? CpuLimit { get; init; }

    [Required]
    public int? MemoryMbLimit { get; init; }

    [Required]
    public int? PidLimit { get; init; }
}