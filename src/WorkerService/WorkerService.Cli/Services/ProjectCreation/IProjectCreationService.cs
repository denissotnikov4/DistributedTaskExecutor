using TaskService.Client.Models.Tasks;

namespace WorkerService.Cli.Services.ProjectCreation;

public interface IProjectCreationService
{
    Task<string> CreateAsync(string name, string sourceCode, ProgrammingLanguage language);
}