using WorkerService.Cli.Helpers;
using WorkerService.Cli.Services.ProjectCreators.Models;

namespace WorkerService.Cli.Services.ProjectCreators.Base;

public abstract class ProjectCreatorBase : IProjectCreator
{
    private static readonly Dictionary<string, string> LanguagesEntryPoints = new()
    {
        [Constants.CSharp.LanguageName] = "Program.cs",
        [Constants.Python.LanguageName] = "main.py"
    };

    private readonly string supportedLanguage;
    private readonly string dockerFile;

    protected ProjectCreatorBase(string supportedLanguage)
    {
        this.supportedLanguage = supportedLanguage ?? throw new ArgumentNullException(nameof(supportedLanguage));
        this.dockerFile = $"{supportedLanguage}.Dockerfile";
    }

    public bool Accept(string language)
    {
        return language == this.supportedLanguage;
    }

    public async Task<string> CreateAsync(string name, string sourceCode)
    {
        var projectPath = CreateProjectDirectory(name);

        await this.InitProjectIfNeededAsync(projectPath);

        await this.AddCodeToProject(sourceCode, projectPath);

        await this.AddDockerfileToProject(projectPath);

        return projectPath;
    }

    private static string CreateProjectDirectory(string name)
    {
        var projectPath = Path.Combine(Path.GetTempPath(), name);

        Directory.CreateDirectory(projectPath);

        return projectPath;
    }

    private async Task AddCodeToProject(string sourceCode, string projectPath)
    {
        await File.WriteAllTextAsync(
            Path.Combine(projectPath, LanguagesEntryPoints[this.supportedLanguage]),
            sourceCode);
    }

    private async Task AddDockerfileToProject(string projectPath)
    {
        await File.WriteAllTextAsync(
            Path.Combine(projectPath, "Dockerfile"),
            ResourcesHelper.GetResource(this.dockerFile));
    }

    protected virtual Task InitProjectIfNeededAsync(string projectPath)
    {
        return Task.CompletedTask;
    }
}