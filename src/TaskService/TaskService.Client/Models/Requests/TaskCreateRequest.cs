using System.ComponentModel.DataAnnotations;

namespace TaskService.Client.Models.Requests;

public class TaskCreateRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    public string? Data { get; set; }

    public Guid UserId { get; set; }

    public TimeSpan Ttl { get; set; }
}