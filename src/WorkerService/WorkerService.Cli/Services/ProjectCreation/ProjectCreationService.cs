using TaskService.Client.Models.Tasks;

namespace WorkerService.Cli.Services.ProjectCreation;

public class ProjectCreationService : IProjectCreationService
{
    private static readonly Dictionary<ProgrammingLanguage, string> LanguagesEntryPoints = new()
    {
        [ProgrammingLanguage.CSharp] = "Program.cs",
        [ProgrammingLanguage.Python] = "main.py"
    };

    public async Task<string> CreateAsync(string name, string sourceCode, ProgrammingLanguage language)
    {
        var projectPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), name)).FullName;

        await File.WriteAllTextAsync(
            Path.Combine(projectPath, GetLanguageEntryPoint(language)),
            sourceCode);

        return projectPath;
    }

    private static string GetLanguageEntryPoint(ProgrammingLanguage language)
    {
        if (!LanguagesEntryPoints.TryGetValue(language, out var entryPoint))
        {
            throw new ArgumentException($"No entry point found for language: {language}.");
        }

        return entryPoint;
    }
}