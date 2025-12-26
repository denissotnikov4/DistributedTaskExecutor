using ApiKeys.Logic.Services.ApiKeys;

namespace ApiKeys.UnitTests.Logic.Services.ApiKeys;

[TestFixture]
public class ApiKeysHasherTests
{
    [Test]
    public void ComputeHash_WithValidInput_ReturnsBase64String()
    {
        // Arrange
        const string input = "test-api-key";

        // Act
        var hash = ApiKeysHasher.ComputeHash(input);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(hash).Should().NotBeNull();
    }

    [Test]
    public void ComputeHash_WithSameInput_ReturnsSameHash()
    {
        // Arrange
        const string input = "test-api-key";

        // Act
        var hash1 = ApiKeysHasher.ComputeHash(input);
        var hash2 = ApiKeysHasher.ComputeHash(input);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Test]
    public void ComputeHash_WithDifferentInputs_ReturnsDifferentHashes()
    {
        // Arrange
        const string input1 = "test-api-key-1";
        const string input2 = "test-api-key-2";

        // Act
        var hash1 = ApiKeysHasher.ComputeHash(input1);
        var hash2 = ApiKeysHasher.ComputeHash(input2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Test]
    public void ComputeHash_WithEmptyString_ReturnsValidHash()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var hash = ApiKeysHasher.ComputeHash(input);

        // Assert
        hash.Should().NotBeNull();
        Convert.FromBase64String(hash).Should().NotBeNull();
    }

    [Test]
    public void ComputeHash_WithLongString_ReturnsValidHash()
    {
        // Arrange
        var input = new string('a', 10000);

        // Act
        var hash = ApiKeysHasher.ComputeHash(input);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(hash).Should().NotBeNull();
    }

    [Test]
    public void ComputeHash_WithSpecialCharacters_ReturnsValidHash()
    {
        // Arrange
        const string input = "test-key-with-!@#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        var hash = ApiKeysHasher.ComputeHash(input);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(hash).Should().NotBeNull();
    }
}

