using TaskService.Client.Models.Tasks;

namespace WorkerService.Cli.Services.ProjectCreators.Base;

public interface IProjectCreator
{
    bool Accept(ProgrammingLanguage language);

    Task<string> CreateAsync(string name, string sourceCode);
}