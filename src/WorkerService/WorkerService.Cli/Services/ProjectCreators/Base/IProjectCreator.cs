using TaskService.Client.Models.Tasks;

namespace WorkerService.Cli.Services.ProjectCreators.Base;

/// <summary>
/// Класс, создающий проект с кодом для выбранного языка.
/// </summary>
public interface IProjectCreator
{
    /// <summary>
    /// Подходит ли данный ProjectCreator переданному ЯП.
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    bool Accept(ProgrammingLanguage language);

    /// <summary>
    /// Создать проект с кодом для выбранного языка.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sourceCode"></param>
    /// <returns>Абсолютный путь до проекта.</returns>
    Task<string> CreateAsync(string name, string sourceCode);
}