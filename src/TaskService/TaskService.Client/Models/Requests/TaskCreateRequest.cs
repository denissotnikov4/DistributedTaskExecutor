using System.ComponentModel.DataAnnotations;
using TaskService.Client.Models.Tasks;

namespace TaskService.Client.Models.Requests;

public class TaskCreateRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    [Required]
    public ProgrammingLanguage? Language { get; set; }

    public string? InputData { get; set; }

    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(1);
}