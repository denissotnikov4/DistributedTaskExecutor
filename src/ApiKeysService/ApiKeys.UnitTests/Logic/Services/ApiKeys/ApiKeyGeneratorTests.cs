using ApiKeys.Logic.Services.ApiKeys;

namespace ApiKeys.UnitTests.Logic.Services.ApiKeys;

[TestFixture]
public class ApiKeyGeneratorTests
{
    [Test]
    public void Generate_ReturnsNonEmptyString()
    {
        // Arrange
        var generator = new ApiKeyGenerator();

        // Act
        var apiKey = generator.Generate();

        // Assert
        apiKey.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Generate_ReturnsKeyWithPrefix()
    {
        // Arrange
        var generator = new ApiKeyGenerator();

        // Act
        var apiKey = generator.Generate();

        // Assert
        apiKey.Should().StartWith("ak_");
    }

    [Test]
    public void Generate_ReturnsKeyWithCorrectLength()
    {
        // Arrange
        var generator = new ApiKeyGenerator();
        const int expectedLength = 32;

        // Act
        var apiKey = generator.Generate();

        // Assert
        apiKey.Length.Should().Be(expectedLength);
    }

    [Test]
    public void Generate_ReturnsDifferentKeysOnEachCall()
    {
        // Arrange
        var generator = new ApiKeyGenerator();
        var keys = new HashSet<string>();

        // Act
        for (var i = 0; i < 100; i++)
        {
            keys.Add(generator.Generate());
        }

        // Assert
        keys.Should().HaveCount(100, because: "All generated keys should be unique");
    }

    [Test]
    public void Generate_ReturnsKeyWithoutInvalidBase64Characters()
    {
        // Arrange
        var generator = new ApiKeyGenerator();

        // Act
        var apiKey = generator.Generate();

        // Assert
        apiKey.Should().NotContain("+");
        apiKey.Should().NotContain("/");
    }

    [Test]
    public void Generate_ReturnsKeyWithValidCharacters()
    {
        // Arrange
        var generator = new ApiKeyGenerator();

        // Act
        var apiKey = generator.Generate();

        // Assert
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
        apiKey.All(c => validChars.Contains(c)).Should().BeTrue();
    }
}

