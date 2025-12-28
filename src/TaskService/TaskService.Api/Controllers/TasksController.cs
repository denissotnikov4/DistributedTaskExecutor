using ApiKeys.Client.Auth;
using Microsoft.AspNetCore.Mvc;
using TaskService.Api.Constants;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;
using TaskService.Client.Models.Tasks.Responses;
using TaskService.Logic.Services.Tasks;

namespace TaskService.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[ApiKeyRequired]
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
    [ApiKeyRequired(RequiredClaims = [ApiKeyClaims.TasksWrite])]
    [ProducesResponseType(typeof(TaskCreateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateTaskAsync([FromBody] TaskCreateRequest request)
    {
        var taskId = await this.taskService.CreateTaskAsync(request);

        return this.Ok(new TaskCreateResponse { Id = taskId });
    }

    /// <summary>
    /// Получить задачу по ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ApiKeyRequired(RequiredClaims = [ApiKeyClaims.TasksRead])]
    [ProducesResponseType(typeof(ClientTask), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetTaskByIdAsync(Guid id)
    {
        var foundTask = await this.taskService.GetTaskByIdAsync(id);

        return foundTask == null ? this.NotFound() : this.Ok(foundTask);
    }

    /// <summary>
    /// Обновить задачу по ID.
    /// </summary>
    [HttpPatch("{id}")]
    [ApiKeyRequired(RequiredClaims = [ApiKeyClaims.TasksWrite])]
    [ProducesResponseType(typeof(ClientTask), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateTaskAsync(Guid id, TaskUpdateRequest updateRequest)
    {
        await this.taskService.UpdateTaskAsync(id, updateRequest);

        return this.Ok();
    }

    /// <summary>
    /// Получить все задачи.
    /// </summary>
    [HttpGet]
    [ApiKeyRequired(RequiredClaims = [ApiKeyClaims.TasksRead])]
    [ProducesResponseType(typeof(IEnumerable<ClientTask>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetAllTasksAsync()
    {
        return this.Ok(await this.taskService.GetAllTasksAsync());
    }

    /// <summary>
    /// Повторить выполнение задачи.
    /// </summary>
    [HttpPost("{id}/retry")]
    [ApiKeyRequired(RequiredClaims = [ApiKeyClaims.TasksWrite])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RetryTaskAsync(Guid id)
    {
        await this.taskService.RetryTaskAsync(id);

        return this.Ok();
    }
}