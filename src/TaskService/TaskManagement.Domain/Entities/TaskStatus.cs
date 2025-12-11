namespace TaskManagement.Domain.Entities;

public enum TaskStatus
{
    Pending = 0,      // В ожидании
    InProgress = 1,   // В процессе выполнения
    Completed = 2,    // Завершена успешно
    Expired = 3,      // Истекла (TTL)
    Failed = 4,       // Завершена с ошибкой
    Cancelled = 5     // Отменена
}

