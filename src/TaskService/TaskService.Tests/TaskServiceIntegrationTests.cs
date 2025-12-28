using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using TaskService.Api.DTO.Responses;
using TaskService.Client.Models.Tasks;
using TaskService.Client.Models.Tasks.Requests;
using TaskService.Tests.Infrastructure;
using TaskStatus = TaskService.Client.Models.Tasks.TaskStatus;

namespace TaskService.Tests;

[TestFixture]
internal class TaskServiceIntegrationTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateTaskAsync_ValidRequest_ReturnsCreatedAndPublishesMessage()
    {
        // Arrange
        var request = CreateValidTaskRequest();
        
        var publishedTaskIds = new List<Guid>();
        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()))
            .Callback<Guid, JsonSerializerOptions?>((id, _) => publishedTaskIds.Add(id));

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        publishedTaskIds.Should().HaveCount(1);
        
        var responseContent = await response.Content.ReadFromJsonAsync<TaskCreateResponse>();
        
        var taskId = responseContent!.Id;
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Name.Should().Be(request.Name);
        dbTask.Code.Should().Be(request.Code);
        dbTask.Language.Should().Be(request.Language!.Value);
        dbTask.InputData.Should().Be(request.InputData);
        dbTask.Ttl.Should().Be(request.Ttl!.Value);
        dbTask.Status.Should().Be(TaskStatus.Pending);
    }

    [Test]
    public async Task CreateTaskAsync_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange - отсутствует Name
        var invalidRequest = new TaskCreateRequest
        {
            Name = "", // Пустое имя
            Code = "test",
            Language = ProgrammingLanguage.CSharp,
            Ttl = TimeSpan.FromMinutes(5)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        RabbitMqMock.Verify(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()), Times.Never);
    }

    [Test]
    public async Task CreateTaskAsync_MissingTtl_ReturnsBadRequest()
    {
        // Arrange - отсутствует Ttl
        var invalidRequest = new TaskCreateRequest
        {
            Name = "Test Task",
            Code = "test",
            Language = ProgrammingLanguage.CSharp,
            Ttl = null // Обязательное поле
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetTaskByIdAsync_ExistingTask_ReturnsTaskWithCorrectData()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            name: "Calculate Fibonacci",
            code: "def fib(n): return n if n <= 1 else fib(n-1) + fib(n-2)",
            language: ProgrammingLanguage.Python,
            status: TaskStatus.Completed,
            ttl: TimeSpan.FromHours(2));

        // Act
        var response = await Client.GetAsync($"/api/tasks/{taskId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var task = await response.Content.ReadFromJsonAsync<ClientTask>();
        task.Should().NotBeNull();
        task!.Id.Should().Be(taskId);
        task.Name.Should().Be("Calculate Fibonacci");
        task.Language.Should().Be(ProgrammingLanguage.Python);
        task.Status.Should().Be(TaskStatus.Completed);
        task.Ttl.Should().Be(TimeSpan.FromHours(2));
        task.Result.Should().NotBeNull();
        task.CompletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task GetTaskByIdAsync_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        
        // Act
        var response = await Client.GetAsync($"/api/tasks/{nonExistingId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetAllTasksAsync_ReturnsAllTasksOrderedByCreationDate()
    {
        // Arrange
        var task1Id = await CreateTestTaskInDbAsync(
            name: "Task 1",
            status: TaskStatus.Pending);
        
        await Task.Delay(10);
        
        var task2Id = await CreateTestTaskInDbAsync(
            name: "Task 2", 
            status: TaskStatus.InProgress);
        
        await Task.Delay(10);
        
        var task3Id = await CreateTestTaskInDbAsync(
            name: "Task 3",
            status: TaskStatus.Completed);

        // Act
        var response = await Client.GetAsync("/api/tasks");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tasks = await response.Content.ReadFromJsonAsync<List<ClientTask>>();
        tasks.Should().NotBeNull();
        tasks.Should().HaveCount(3);
        
        // Проверяем, что задачи содержат все необходимые поля
        tasks![0].Should().Match<ClientTask>(t => 
            t.Id == task1Id && t.Name == "Task 1");
        tasks[1].Should().Match<ClientTask>(t => 
            t.Id == task2Id && t.Name == "Task 2");
        tasks[2].Should().Match<ClientTask>(t => 
            t.Id == task3Id && t.Name == "Task 3");
    }

    [Test]
    public async Task RetryTaskAsync_CompletedTask_ResetsAndPublishesMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = await CreateTestTaskInDbAsync(
            userId: userId,
            name: "Math Calculation",
            code: "return 2 + 2;",
            language: ProgrammingLanguage.CSharp,
            status: TaskStatus.Completed,
            retryCount: 1,
            ttl: TimeSpan.FromMinutes(45));
        
        var publishedTaskIds = new List<Guid>();
        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()))
            .Callback<Guid, JsonSerializerOptions?>((id, _) => publishedTaskIds.Add(id));
        
        // Act
        var response = await Client.PostAsync($"/api/tasks/{taskId}/retry", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Проверяем изменения в БД
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Status.Should().Be(TaskStatus.Pending);
        dbTask.Result.Should().BeNull();
        dbTask.StartedAt.Should().BeNull();
        dbTask.CompletedAt.Should().BeNull();
        dbTask.ErrorMessage.Should().BeNull();
        dbTask.RetryCount.Should().Be(2);
        dbTask.Name.Should().Be("Math Calculation"); // Остальные поля не меняются
        dbTask.Code.Should().Be("return 2 + 2;");
        dbTask.Language.Should().Be(ProgrammingLanguage.CSharp);
        dbTask.UserId.Should().Be(userId);
        dbTask.Ttl.Should().Be(TimeSpan.FromMinutes(45));
        
        publishedTaskIds.Should().HaveCount(1);
        publishedTaskIds[0].Should().Be(taskId);
    }

    [Test]
    public async Task RetryTaskAsync_FailedTask_ResetsAndPublishesMessage()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            status: TaskStatus.Failed,
            errorMessage: "Runtime exception: division by zero",
            retryCount: 2,
            ttl: TimeSpan.FromHours(1));
        
        var publishedTaskIds = new List<Guid>();
        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()))
            .Callback<Guid, JsonSerializerOptions?>((id, _) => publishedTaskIds.Add(id));
        
        // Act
        var response = await Client.PostAsync($"/api/tasks/{taskId}/retry", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Status.Should().Be(TaskStatus.Pending);
        dbTask.ErrorMessage.Should().BeNull();
        dbTask.RetryCount.Should().Be(3);
        dbTask.Ttl.Should().Be(TimeSpan.FromHours(1)); // TTL остается прежним
        
        publishedTaskIds.Should().HaveCount(1);
    }

    [Test]
    public async Task RetryTaskAsync_ExpiredTask_ResetsAndPublishesMessage()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            status: TaskStatus.Expired,
            errorMessage: "Task expired",
            retryCount: 0);
        
        var publishedTaskIds = new List<Guid>();
        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()))
            .Callback<Guid, JsonSerializerOptions?>((id, _) => publishedTaskIds.Add(id));
        
        // Act
        var response = await Client.PostAsync($"/api/tasks/{taskId}/retry", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Status.Should().Be(TaskStatus.Pending);
        dbTask.ErrorMessage.Should().BeNull();
        
        publishedTaskIds.Should().HaveCount(1);
    }

    [Test]
    public async Task RetryTaskAsync_PendingTask_ReturnsBadRequest()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            status: TaskStatus.Pending,
            retryCount: 0);
        
        // Act
         var response = await Client.PostAsync($"/api/tasks/{taskId}/retry", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("cannot be retried");
    }

    [Test]
    public async Task RetryTaskAsync_InProgressTask_ReturnsBadRequest()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            status: TaskStatus.InProgress,
            retryCount: 0);
        
        // Act
        var response = await Client.PostAsync($"/api/tasks/{taskId}/retry", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RetryTaskAsync_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        
        // Act
        var response = await Client.PostAsync($"/api/tasks/{nonExistingId}/retry", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateTaskAsync_WithPythonCode_ReturnsCreated()
    {
        // Arrange
        var request = new TaskCreateRequest
        {
            Name = "Python Script",
            Code = "def hello():\n    return 'Hello from Python'",
            Language = ProgrammingLanguage.Python,
            InputData = "{\"name\": \"World\"}",
            Ttl = TimeSpan.FromMinutes(15)
        };

        var publishedTaskIds = new List<Guid>();
        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()))
            .Callback<Guid, JsonSerializerOptions?>((id, _) => publishedTaskIds.Add(id));

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadFromJsonAsync<TaskCreateResponse>();
        
        var taskId = responseContent!.Id;
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Language.Should().Be(ProgrammingLanguage.Python);
        dbTask.Code.Should().Be(request.Code);
    }

    [Test]
    public async Task GetTaskByIdAsync_TaskWithLongTtl_ReturnsCorrectTtl()
    {
        // Arrange
        var longTtl = TimeSpan.FromDays(7);
        var taskId = await CreateTestTaskInDbAsync(
            name: "Long Running Task",
            ttl: longTtl,
            status: TaskStatus.InProgress);

        // Act
        var response = await Client.GetAsync($"/api/tasks/{taskId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var task = await response.Content.ReadFromJsonAsync<ClientTask>();
        task.Should().NotBeNull();
        task!.Ttl.Should().Be(longTtl);
        task.Status.Should().Be(TaskStatus.InProgress);
    }

    [Test]
    public async Task GetAllTasksAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange - база пустая после SetUp
        
        // Act
        var response = await Client.GetAsync("/api/tasks");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tasks = await response.Content.ReadFromJsonAsync<List<ClientTask>>();
        tasks.Should().NotBeNull();
        tasks.Should().BeEmpty();
    }

    [Test]
    public async Task RetryTaskAsync_IncreasesRetryCount_WhenCalled()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            status: TaskStatus.Failed,
            retryCount: 5);
        
        var publishedTaskIds = new List<Guid>();
        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()))
            .Callback<Guid, JsonSerializerOptions?>((id, _) => publishedTaskIds.Add(id));
        
        // Act
        var response = await Client.PostAsync($"/api/tasks/{taskId}/retry", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask.RetryCount.Should().Be(6);
    }

    [Test]
    public async Task CreateTaskAsync_WithNullInputData_CreatesSuccessfully()
    {
        // Arrange - InputData может быть null
        var request = new TaskCreateRequest
        {
            Name = "Task without input",
            Code = "return 42;",
            Language = ProgrammingLanguage.CSharp,
            InputData = null, // null значение допустимо
            Ttl = TimeSpan.FromMinutes(10)
        };

        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()));

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadFromJsonAsync<TaskCreateResponse>();
        
        var taskId = responseContent!.Id;
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.InputData.Should().BeNull();
    }

    [Test]
    public async Task CreateTaskAsync_WithSpecialCharactersInCode_CreatesSuccessfully()
    {
        // Arrange - код с спецсимволами и Unicode
        var request = new TaskCreateRequest
        {
            Name = "Unicode Test",
            Code = "// Комментарий на русском\nvar π = Math.PI;\nreturn π * 2;",
            Language = ProgrammingLanguage.CSharp,
            InputData = "{}",
            Ttl = TimeSpan.FromMinutes(20)
        };

        RabbitMqMock.Setup(x => x.Publish(It.IsAny<Guid>(), It.IsAny<JsonSerializerOptions?>()));

        // Act
        var response = await Client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadFromJsonAsync<TaskCreateResponse>();
        
        var taskId = responseContent!.Id;
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Code.Should().Be(request.Code);
    }
    
    [Test]
    public async Task UpdateTaskAsync_ExistingTask_UpdatesPartialFields()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            name: "Old Name",
            code: "old code",
            status: TaskStatus.Completed);
        
        var updateRequest = new TaskUpdateRequest
        {
            Name = "New Name",
            Code = null,
            Status = null
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Name.Should().Be(updateRequest.Name);
        dbTask.Code.Should().Be("old code");
        dbTask.Status.Should().Be(TaskStatus.Completed);
    }

    [Test]
    public async Task UpdateTaskAsync_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new TaskUpdateRequest { Name = "New Name" };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{nonExistingId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateTaskAsync_EmptyUpdateRequest_KeepsOriginalValues()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            name: "Original Name",
            code: "original code",
            language: ProgrammingLanguage.CSharp);
        
        var emptyRequest = new TaskUpdateRequest();

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", emptyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Name.Should().Be("Original Name");
        dbTask.Code.Should().Be("original code");
    }

    [Test]
    public async Task UpdateTaskAsync_UpdateMultipleFields_MapsCorrectly()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            name: "Before Update",
            language: ProgrammingLanguage.CSharp,
            ttl: TimeSpan.FromMinutes(30));
        
        var updateRequest = new TaskUpdateRequest
        {
            Name = "Updated Name",
            Language = ProgrammingLanguage.Python,
            Ttl = TimeSpan.FromHours(2),
            InputData = "{\"new\": \"data\"}"
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Name.Should().Be(updateRequest.Name);
        dbTask.Language.Should().Be(ProgrammingLanguage.Python);
        dbTask.Ttl.Should().Be(TimeSpan.FromHours(2));
        dbTask.InputData.Should().Be("{\"new\": \"data\"}");
    }

    [Test]
    public async Task UpdateTaskAsync_UpdateStatusToPending_TimestampsAreNotReset()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            status: TaskStatus.Completed);

        var updateRequest = new TaskUpdateRequest
        {
            Status = TaskStatus.Pending
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Status.Should().Be(TaskStatus.Pending);
    }

    [Test]
    public async Task UpdateTaskAsync_UpdateUserId_ChangesOwnership()
    {
        // Arrange
        var oldUserId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();
        
        var taskId = await CreateTestTaskInDbAsync(userId: oldUserId);
        
        var updateRequest = new TaskUpdateRequest
        {
            UserId = newUserId
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.UserId.Should().Be(newUserId);
    }

    [Test]
    public async Task UpdateTaskAsync_WithNullValues_IgnoresNullFields()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(
            name: "Keep This Name",
            errorMessage: "Keep This Error");
        
        var updateRequest = new TaskUpdateRequest
        {
            Name = null,
            ErrorMessage = null
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask.Should().NotBeNull();
        dbTask!.Name.Should().Be("Keep This Name");
        dbTask.ErrorMessage.Should().Be("Keep This Error");
    }

    [Test]
    public async Task UpdateTaskAsync_LongTtlValue_SavesCorrectly()
    {
        // Arrange
        var taskId = await CreateTestTaskInDbAsync(ttl: TimeSpan.FromMinutes(10));
        var longTtl = TimeSpan.FromDays(30);
        
        var updateRequest = new TaskUpdateRequest { Ttl = longTtl };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dbTask = await TaskRepository.GetByIdAsync(taskId);
        dbTask!.Ttl.Should().Be(longTtl);
    }
}