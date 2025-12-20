namespace WorkerService.Cli.Services.ProjectCreators.Models;

public static class Constants
{
    public static class CSharp
    {
        // Может стоит завести Enum?
        public const string LanguageName = "csharp";

        public const string EntryPoint = "Program.cs";
    }

    public static class Python
    {
        public const string LanguageName = "python";

        public const string EntryPoint = "main.py";
    }
}