namespace TaskService.Client.Models.Tasks;

public enum TaskStatus
{
    /// <summary>
    /// Ожидает начала выполнения.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Выполняется.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Успешно завершилась.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Завершилась с ошибкой.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Истекла.
    /// </summary>
    Expired = 4
}