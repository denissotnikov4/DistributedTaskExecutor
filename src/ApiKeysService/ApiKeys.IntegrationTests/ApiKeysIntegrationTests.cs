using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ApiKeys.Client.Models;
using ApiKeys.IntegrationTests.Infrastructure;

namespace ApiKeys.IntegrationTests;

[TestFixture]
public class ApiKeysIntegrationTests : BaseIntegrationTest
{
    [Test]
    public async Task CreateApiKeyAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = this.CreateValidApiKeyRequest();

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiKeyCreateResponse>();
        responseContent.Should().NotBeNull();
        responseContent.Id.Should().NotBeEmpty();
        responseContent.ApiKey.Should().NotBeNullOrEmpty();
        responseContent.Info.Should().NotBeNull();
        responseContent.Info.Name.Should().Be(request.Name);
        responseContent.Info.ExpiresAt.Should().BeCloseTo(request.ExpiresAt!.Value, TimeSpan.FromSeconds(1));
        responseContent.Info.IsActive.Should().BeTrue();
        responseContent.Info.Claims.Should().BeEquivalentTo(request.Claims);

        var dbApiKey = await this.ApiKeysRepository.GetByIdAsync(responseContent.Id);
        dbApiKey.Should().NotBeNull();
        dbApiKey.Name.Should().Be(request.Name);
        dbApiKey.ExpiresAt.Should().BeCloseTo(request.ExpiresAt!.Value, TimeSpan.FromSeconds(1));
        dbApiKey.IsActive.Should().BeTrue();
        dbApiKey.Claims.Should().BeEquivalentTo(request.Claims);
    }

    [Test]
    public async Task CreateApiKeyAsync_WithoutExpiresAt_CreatesSuccessfully()
    {
        // Arrange
        var request = new ApiKeyCreateRequest
        {
            Name = "Non-expiring Key",
            ExpiresAt = null,
            Claims = ["tasks:read"]
        };

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiKeyCreateResponse>();
        responseContent.Should().NotBeNull();
        responseContent.Info.ExpiresAt.Should().BeNull();
    }

    [Test]
    public async Task CreateApiKeyAsync_WithoutClaims_CreatesSuccessfully()
    {
        // Arrange
        var request = new ApiKeyCreateRequest
        {
            Name = "Key without claims",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Claims = null
        };

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadFromJsonAsync<ApiKeyCreateResponse>();
        responseContent.Should().NotBeNull();
        responseContent.Info.Claims.Should().BeEmpty();
    }

    [Test]
    public async Task GetApiKeyByIdAsync_ExistingApiKey_ReturnsApiKeyInfo()
    {
        // Arrange
        var apiKeyId = await this.CreateTestApiKeyInDbAsync(
            name: "Test Key",
            expiresAt: DateTime.UtcNow.AddDays(30),
            isActive: true,
            claims: ["tasks:read", "tasks:write"]);

        // Act
        var response = await this.Client.GetAsync($"/api/apikeys/{apiKeyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var apiKeyInfo = await response.Content.ReadFromJsonAsync<ApiKeyInfo>();
        apiKeyInfo.Should().NotBeNull();
        apiKeyInfo.Id.Should().Be(apiKeyId);
        apiKeyInfo.Name.Should().Be("Test Key");
        apiKeyInfo.IsActive.Should().BeTrue();
        apiKeyInfo.Claims.Should().BeEquivalentTo(new List<string> { "tasks:read", "tasks:write" });
    }

    [Test]
    public async Task GetApiKeyByIdAsync_NonExistingApiKey_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await this.Client.GetAsync($"/api/apikeys/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetAllApiKeysAsync_ReturnsAllApiKeys()
    {
        // Arrange
        var apiKey1Id = await this.CreateTestApiKeyInDbAsync(
            name: "Key 1",
            isActive: true);

        var apiKey2Id = await this.CreateTestApiKeyInDbAsync(
            name: "Key 2",
            isActive: false);

        var apiKey3Id = await this.CreateTestApiKeyInDbAsync(
            name: "Key 3",
            isActive: true);

        // Act
        var response = await this.Client.GetAsync("/api/apikeys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var apiKeys = await response.Content.ReadFromJsonAsync<List<ApiKeyInfo>>();
        apiKeys.Should().NotBeNull();
        apiKeys.Should().HaveCount(3);

        apiKeys.Should().Contain(k => k.Id == apiKey1Id && k.Name == "Key 1");
        apiKeys.Should().Contain(k => k.Id == apiKey2Id && k.Name == "Key 2");
        apiKeys.Should().Contain(k => k.Id == apiKey3Id && k.Name == "Key 3");
    }

    [Test]
    public async Task GetAllApiKeysAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var response = await this.Client.GetAsync("/api/apikeys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var apiKeys = await response.Content.ReadFromJsonAsync<List<ApiKeyInfo>>();
        apiKeys.Should().NotBeNull();
        apiKeys.Should().BeEmpty();
    }

    [Test]
    public async Task UpdateApiKeyAsync_NonExistingApiKey_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new ApiKeyUpdateRequest { Name = "New Name" };

        // Act
        var response = await this.Client.PutAsJsonAsync($"/api/apikeys/{nonExistingId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteApiKeyAsync_ExistingApiKey_DeletesSuccessfully()
    {
        // Arrange
        var apiKeyId = await this.CreateTestApiKeyInDbAsync(
            name: "Key to Delete",
            isActive: true);

        // Act
        var response = await this.Client.DeleteAsync($"/api/apikeys/{apiKeyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbApiKey = await this.ApiKeysRepository.GetByIdAsync(apiKeyId);
        dbApiKey.Should().BeNull();
    }

    [Test]
    public async Task DeleteApiKeyAsync_NonExistingApiKey_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await this.Client.DeleteAsync($"/api/apikeys/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ValidateApiKeyAsync_ValidActiveApiKey_ReturnsSuccess()
    {
        // Arrange
        await this.CreateTestApiKeyInDbAsync(
            name: "Valid Key",
            isActive: true,
            claims: ["tasks:read", "tasks:write"]);

        // Создаем реальный API ключ через сервис
        var request = this.CreateValidApiKeyRequest();
        var createResponse = await this.Client.PostAsJsonAsync("/api/apikeys", request);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiKeyCreateResponse>();
        var apiKey = createResult!.ApiKey;

        var validateRequest = new ValidateApiKeyRequest(apiKey);

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys/validate", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var validationResult = await response.Content.ReadFromJsonAsync<ApiKeyValidationResult>();
        validationResult.Should().NotBeNull();
        validationResult.ApiKeyId.Should().NotBeNull();
        validationResult.Claims.Should().BeEquivalentTo(request.Claims);

        var dbApiKey = await this.ApiKeysRepository.GetByIdAsync(validationResult.ApiKeyId!.Value);
        dbApiKey.Should().NotBeNull();
        dbApiKey.LastUsedAt.Should().NotBeNull();
    }

    [Test]
    public async Task ValidateApiKeyAsync_InvalidApiKey_ReturnsConflict()
    {
        // Arrange
        var validateRequest = new ValidateApiKeyRequest("invalid-key");

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys/validate", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task ValidateApiKeyAsync_InactiveApiKey_ReturnsConflict()
    {
        // Arrange
        var request = this.CreateValidApiKeyRequest();
        var createResponse = await this.Client.PostAsJsonAsync("/api/apikeys", request);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiKeyCreateResponse>();
        var apiKey = createResult!.ApiKey;

        await this.Client.PutAsJsonAsync($"/api/apikeys/{createResult.Id}", new ApiKeyUpdateRequest
        {
            IsActive = false
        });

        var validateRequest = new ValidateApiKeyRequest(apiKey);

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys/validate", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task ValidateApiKeyAsync_ExpiredApiKey_ReturnsConflict()
    {
        // Arrange
        var request = new ApiKeyCreateRequest
        {
            Name = "Expired Key",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Уже истек
            Claims = ["tasks:read"]
        };

        var createResponse = await this.Client.PostAsJsonAsync("/api/apikeys", request);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiKeyCreateResponse>();
        var apiKey = createResult!.ApiKey;

        var validateRequest = new ValidateApiKeyRequest(apiKey);

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys/validate", validateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task CreateApiKeyAsync_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new ApiKeyCreateRequest
        {
            Name = "",
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await this.Client.PostAsJsonAsync("/api/apikeys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UpdateApiKeyAsync_EmptyUpdateRequest_KeepsOriginalValues()
    {
        // Arrange
        var apiKeyId = await this.CreateTestApiKeyInDbAsync(
            name: "Original Name",
            expiresAt: DateTime.UtcNow.AddDays(10),
            isActive: true,
            claims: ["tasks:read"]);

        var emptyRequest = new ApiKeyUpdateRequest();

        // Act
        var response = await this.Client.PutAsJsonAsync($"/api/apikeys/{apiKeyId}", emptyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbApiKey = await this.ApiKeysRepository.GetByIdAsync(apiKeyId);
        dbApiKey.Should().NotBeNull();
        dbApiKey.Name.Should().Be("Original Name");
        dbApiKey.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task GetApiKeyByIdAsync_WithLastUsedAt_ReturnsCorrectTimestamp()
    {
        // Arrange
        var lastUsedAt = DateTime.UtcNow.AddHours(-2);
        var apiKeyId = await this.CreateTestApiKeyInDbAsync(
            name: "Used Key",
            lastUsedAt: lastUsedAt);

        // Act
        var response = await this.Client.GetAsync($"/api/apikeys/{apiKeyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var apiKeyInfo = await response.Content.ReadFromJsonAsync<ApiKeyInfo>();
        apiKeyInfo.Should().NotBeNull();
        apiKeyInfo.LastUsedAt.Should().NotBeNull();
    }
}