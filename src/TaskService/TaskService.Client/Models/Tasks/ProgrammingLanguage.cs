using System.Text.Json.Serialization;

namespace TaskService.Client.Models.Tasks;

/// <summary>
/// Язык программирования.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProgrammingLanguage
{
    /// <summary>
    /// C#.
    /// </summary>
    CSharp,

    /// <summary>
    /// Python.
    /// </summary>
    Python
}