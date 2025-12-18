using Microsoft.AspNetCore.Mvc;
using TaskService.Client.Models.Requests;
using TaskService.Client.Models.Tasks;
using TaskService.Logic.Services.Tasks;

namespace TaskService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService taskService;

    public TasksController(ITaskService taskService)
    {
        this.taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    /// <summary>
    /// Создать новую задачу.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientTask), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateTaskAsync([FromBody] TaskCreateRequest request)
    {
        return this.CreatedAtAction("GetTaskById", new { id = Guid.NewGuid() });
    }

    /// <summary>
    /// Получить задачу по ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientTask), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetTaskByIdAsync(Guid id)
    {
        var foundTask = await this.taskService.GetTaskByIdAsync(id);

        return foundTask == null ? this.NotFound() : this.Ok(foundTask);
    }

    /// <summary>
    /// Получить все задачи.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientTask>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllTasksAsync()
    {
        return this.Ok(await this.taskService.GetAllTasksAsync());
    }

    /// <summary>
    /// Повторить выполнение задачи.
    /// </summary>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RetryTaskAsync(Guid id)
    {
        await this.taskService.RetryTaskAsync(id);

        return this.NoContent();
    }
}