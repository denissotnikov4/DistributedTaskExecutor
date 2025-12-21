using TaskService.Client.Models.Tasks;

namespace WorkerService.Cli.Models;

public static class Constants
{
    public const string DockerFile = "Dockerfile";

    public const string DefaultProjectName = "App";

    public static readonly Dictionary<ProgrammingLanguage, string> LanguagesEntryPoints = new()
    {
        [ProgrammingLanguage.CSharp] = "Program.cs",
        [ProgrammingLanguage.Python] = "main.py"
    };
}