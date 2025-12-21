using TaskService.Client.Models.Tasks;
using WorkerService.Cli.Models;

namespace WorkerService.Cli.Services.ProjectCreators.Base;

public abstract class ProjectCreatorBase : IProjectCreator
{
    private readonly ProgrammingLanguage supportedLanguage;

    protected ProjectCreatorBase(ProgrammingLanguage supportedLanguage)
    {
        this.supportedLanguage = supportedLanguage;
    }

    public bool Accept(ProgrammingLanguage language)
    {
        return language == this.supportedLanguage;
    }

    public async Task<string> CreateAsync(string name, string sourceCode)
    {
        var projectPath = CreateProjectDirectory(name);

        await this.InitProjectIfNeededAsync(projectPath);

        await this.AddCodeToProject(sourceCode, projectPath);

        return projectPath;
    }

    private static string CreateProjectDirectory(string name)
    {
        return Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), name)).FullName;
    }

    private async Task AddCodeToProject(string sourceCode, string projectPath)
    {
        await File.WriteAllTextAsync(
            Path.Combine(projectPath, Constants.LanguagesEntryPoints[this.supportedLanguage]),
            sourceCode);
    }

    protected virtual Task InitProjectIfNeededAsync(string projectPath)
    {
        return Task.CompletedTask;
    }
}