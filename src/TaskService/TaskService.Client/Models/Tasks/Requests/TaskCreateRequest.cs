using System.ComponentModel.DataAnnotations;

namespace TaskService.Client.Models.Tasks.Requests;

public class TaskCreateRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    [Required]
    public ProgrammingLanguage? Language { get; set; }

    public string? InputData { get; set; }

    [Required]
    public TimeSpan? Ttl { get; set; }
}