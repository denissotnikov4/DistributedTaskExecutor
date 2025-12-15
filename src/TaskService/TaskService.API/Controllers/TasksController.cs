using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Client.Models.Requests;
using TaskService.Client.Models.Tasks;
using TaskService.Logic.Services;

namespace TaskService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService taskService;
    private readonly ILogger<TasksController> logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        this.taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Создать новую задачу.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientTask), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientTask>> CreateTask([FromBody] CreateTaskRequest request)
    {
        this.logger.LogInformation("Creating new task: {Description}", request.Description);

        var createdId = await this.taskService.CreateTaskAsync(request);

        return this.CreatedAtAction(nameof(this.GetTask), new { id = createdId });
    }

    /// <summary>
    /// Получить задачу по ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientTask), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientTask>> GetTask(Guid id)
    {
        var result = await this.taskService.GetTaskByIdAsync(id);

        if (result == null)
        {
            return this.NotFound();
        }

        return this.Ok(result);
    }

    /// <summary>
    /// Получить все задачи.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientTask>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientTask>>> GetAllTasks()
    {
        var result = await this.taskService.GetAllTasksAsync();
        return this.Ok(result);
    }

    /// <summary>
    /// Повторить выполнение задачи.
    /// </summary>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientTask>> RetryTask(Guid id)
    {
        try
        {
            await this.taskService.RetryTaskAsync(id);

            return this.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequest(new { error = ex.Message });
        }
    }
}