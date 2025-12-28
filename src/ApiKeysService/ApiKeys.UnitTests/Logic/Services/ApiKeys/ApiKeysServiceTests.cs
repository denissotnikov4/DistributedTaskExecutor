using ApiKeys.Client.Models;
using ApiKeys.Dal.Models;
using ApiKeys.Dal.Repositories;
using ApiKeys.Logic.Services.ApiKeys;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiKeys.UnitTests.Logic.Services.ApiKeys;

[TestFixture]
public class ApiKeysServiceTests
{
    private Mock<IApiKeysUnitOfWork> unitOfWorkMock = null!;
    private Mock<IApiKeyGenerator> apiKeyGeneratorMock = null!;
    private Mock<ILogger<ApiKeysService>> loggerMock = null!;
    private ApiKeysService service = null!;

    [SetUp]
    public void SetUp()
    {
        this.unitOfWorkMock = new Mock<IApiKeysUnitOfWork>();
        this.apiKeyGeneratorMock = new Mock<IApiKeyGenerator>();
        this.loggerMock = new Mock<ILogger<ApiKeysService>>();
        this.service = new ApiKeysService(this.unitOfWorkMock.Object, this.apiKeyGeneratorMock.Object, this.loggerMock.Object);
    }

    #region CreateApiKeyAsync Tests

    [Test]
    public async Task CreateApiKeyAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new ApiKeyCreateRequest
        {
            Name = "Test Key",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Claims = ["claim1", "claim2"]
        };
        const string generatedKey = "ak_test12345678901234567890";

        this.apiKeyGeneratorMock.Setup(x => x.Generate()).Returns(generatedKey);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(Mock.Of<IApiKeysRepository>());
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.CreateApiKeyAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ApiKey.Should().Be(generatedKey);
        result.Value.Info.Name.Should().Be(request.Name);
        result.Value.Info.ExpiresAt.Should().Be(request.ExpiresAt);
        result.Value.Info.Claims.Should().BeEquivalentTo(request.Claims);
        result.Value.Info.IsActive.Should().BeTrue();

        this.apiKeyGeneratorMock.Verify(x => x.Generate(), Times.Once);
        this.unitOfWorkMock.Verify(x => x.ApiKeys.Create(It.IsAny<ApiKey>()), Times.Once);
        this.unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateApiKeyAsync_WithNullClaims_ReturnsSuccess()
    {
        // Arrange
        var request = new ApiKeyCreateRequest
        {
            Name = "Test Key",
            ExpiresAt = null,
            Claims = null
        };
        const string generatedKey = "ak_test12345678901234567890";

        this.apiKeyGeneratorMock.Setup(x => x.Generate()).Returns(generatedKey);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(Mock.Of<IApiKeysRepository>());
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.CreateApiKeyAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Info.Claims.Should().BeEmpty();
    }

    [Test]
    public async Task CreateApiKeyAsync_WithEmptyClaims_ReturnsSuccess()
    {
        // Arrange
        var request = new ApiKeyCreateRequest
        {
            Name = "Test Key",
            ExpiresAt = null,
            Claims = []
        };
        const string generatedKey = "ak_test12345678901234567890";

        this.apiKeyGeneratorMock.Setup(x => x.Generate()).Returns(generatedKey);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(Mock.Of<IApiKeysRepository>());
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.CreateApiKeyAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Info.Claims.Should().BeEmpty();
    }

    #endregion

    #region GetApiKeyInfoAsync Tests

    [Test]
    public async Task GetApiKeyInfoAsync_WithExistingId_ReturnsApiKeyInfo()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();
        var apiKey = new ApiKey
        {
            Id = apiKeyId,
            KeyHash = "test-hash",
            Name = "Test Key",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            LastUsedAt = null,
            Claims = ["claim1"]
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(apiKeyId)).ReturnsAsync(apiKey);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.GetApiKeyInfoAsync(apiKeyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(apiKeyId);
        result.Value.Name.Should().Be(apiKey.Name);
    }

    [Test]
    public async Task GetApiKeyInfoAsync_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();
        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(apiKeyId)).ReturnsAsync((ApiKey?)null);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.GetApiKeyInfoAsync(apiKeyId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(apiKeyId.ToString());
    }

    #endregion

    #region GetAllApiKeysAsync Tests

    [Test]
    public async Task GetAllApiKeysAsync_WithExistingKeys_ReturnsAllKeys()
    {
        // Arrange
        var apiKeys = new List<ApiKey>
        {
            new() { Id = Guid.NewGuid(), Name = "Key 1", CreatedAt = DateTime.UtcNow, Claims = [] },
            new() { Id = Guid.NewGuid(), Name = "Key 2", CreatedAt = DateTime.UtcNow, Claims = [] }
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(apiKeys);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.GetAllApiKeysAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAllApiKeysAsync_WithNoKeys_ReturnsEmptyCollection()
    {
        // Arrange
        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ApiKey>());
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.GetAllApiKeysAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region UpdateApiKeyAsync Tests

    [Test]
    public async Task UpdateApiKeyAsync_WithExistingId_ReturnsSuccess()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();
        var apiKey = new ApiKey
        {
            Id = apiKeyId,
            Name = "Old Name",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Claims = []
        };
        var request = new ApiKeyUpdateRequest
        {
            Name = "New Name",
            IsActive = false
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(apiKeyId)).ReturnsAsync(apiKey);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.UpdateApiKeyAsync(apiKeyId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        apiKey.Name.Should().Be("New Name");
        apiKey.IsActive.Should().BeFalse();
        this.unitOfWorkMock.Verify(x => x.ApiKeys.Update(apiKey), Times.Once);
        this.unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateApiKeyAsync_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();
        var request = new ApiKeyUpdateRequest { Name = "New Name" };
        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(apiKeyId)).ReturnsAsync((ApiKey?)null);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.UpdateApiKeyAsync(apiKeyId, request);

        // Assert
        result.IsFailure.Should().BeTrue();
        this.unitOfWorkMock.Verify(x => x.ApiKeys.Update(It.IsAny<ApiKey>()), Times.Never);
    }

    [Test]
    public async Task UpdateApiKeyAsync_WithPartialUpdate_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();
        var originalExpiresAt = DateTime.UtcNow.AddDays(30);
        var apiKey = new ApiKey
        {
            Id = apiKeyId,
            Name = "Original Name",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = originalExpiresAt,
            IsActive = true,
            Claims = ["original"]
        };
        var request = new ApiKeyUpdateRequest
        {
            Name = "Updated Name"
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(apiKeyId)).ReturnsAsync(apiKey);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.UpdateApiKeyAsync(apiKeyId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        apiKey.Name.Should().Be("Updated Name");
        apiKey.ExpiresAt.Should().Be(originalExpiresAt);
        apiKey.IsActive.Should().BeTrue();
        apiKey.Claims.Should().Contain("original");
    }

    #endregion

    #region DeleteApiKeyAsync Tests

    [Test]
    public async Task DeleteApiKeyAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();
        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.DeleteAsync(apiKeyId)).Returns(Task.CompletedTask);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.DeleteApiKeyAsync(apiKeyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        repositoryMock.Verify(x => x.DeleteAsync(apiKeyId), Times.Once);
        this.unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region ValidateApiKeyAsync Tests

    [Test]
    public async Task ValidateApiKeyAsync_WithValidActiveKey_ReturnsSuccess()
    {
        // Arrange
        var apiKey = "ak_test12345678901234567890";
        var apiKeyId = Guid.NewGuid();
        var apiKeyEntity = new ApiKey
        {
            Id = apiKeyId,
            KeyHash = ApiKeysHasher.ComputeHash(apiKey),
            Name = "Test Key",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = null,
            IsActive = true,
            Claims = ["claim1", "claim2"]
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByKeyHashAsync(It.IsAny<string>())).ReturnsAsync(apiKeyEntity);
        repositoryMock.Setup(x => x.UpdateLastUsedAsync(apiKeyId, It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.ValidateApiKeyAsync(apiKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ApiKeyId.Should().Be(apiKeyId);
        result.Value.Claims.Should().BeEquivalentTo(apiKeyEntity.Claims);
        repositoryMock.Verify(x => x.UpdateLastUsedAsync(apiKeyId, It.IsAny<DateTime>()), Times.Once);
        this.unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ValidateApiKeyAsync_WithNonExistingKey_ReturnsConflict()
    {
        // Arrange
        var apiKey = "ak_invalid12345678901234567890";
        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByKeyHashAsync(It.IsAny<string>())).ReturnsAsync((ApiKey?)null);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.ValidateApiKeyAsync(apiKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not valid");
    }

    [Test]
    public async Task ValidateApiKeyAsync_WithInactiveKey_ReturnsConflict()
    {
        // Arrange
        var apiKey = "ak_test12345678901234567890";
        var apiKeyEntity = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = ApiKeysHasher.ComputeHash(apiKey),
            Name = "Test Key",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = null,
            IsActive = false,
            Claims = []
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByKeyHashAsync(It.IsAny<string>())).ReturnsAsync(apiKeyEntity);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.ValidateApiKeyAsync(apiKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("inactive");
    }

    [Test]
    public async Task ValidateApiKeyAsync_WithExpiredKey_ReturnsConflict()
    {
        // Arrange
        var apiKey = "ak_test12345678901234567890";
        var apiKeyEntity = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = ApiKeysHasher.ComputeHash(apiKey),
            Name = "Test Key",
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            Claims = []
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByKeyHashAsync(It.IsAny<string>())).ReturnsAsync(apiKeyEntity);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);

        // Act
        var result = await this.service.ValidateApiKeyAsync(apiKey);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("expired");
    }

    [Test]
    public async Task ValidateApiKeyAsync_WithValidKeyWithExpirationInFuture_ReturnsSuccess()
    {
        // Arrange
        var apiKey = "ak_test12345678901234567890";
        var apiKeyId = Guid.NewGuid();
        var apiKeyEntity = new ApiKey
        {
            Id = apiKeyId,
            KeyHash = ApiKeysHasher.ComputeHash(apiKey),
            Name = "Test Key",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            Claims = ["claim1"]
        };

        var repositoryMock = new Mock<IApiKeysRepository>();
        repositoryMock.Setup(x => x.GetByKeyHashAsync(It.IsAny<string>())).ReturnsAsync(apiKeyEntity);
        repositoryMock.Setup(x => x.UpdateLastUsedAsync(apiKeyId, It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        this.unitOfWorkMock.Setup(x => x.ApiKeys).Returns(repositoryMock.Object);
        this.unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await this.service.ValidateApiKeyAsync(apiKey);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ApiKeyId.Should().Be(apiKeyId);
    }

    #endregion
}

