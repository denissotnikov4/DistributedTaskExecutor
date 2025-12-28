using ApiKeys.Dal.Models;
using ApiKeys.Logic.Services.ApiKeys;

namespace ApiKeys.UnitTests.Logic.Services.ApiKeys;

[TestFixture]
public class ApiKeyExtensionsTests
{
    private Fixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new Fixture();
    }

    [Test]
    public void MapToInfo_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var apiKey = this.fixture.Create<ApiKey>();

        // Act
        var info = apiKey.MapToInfo();

        // Assert
        info.Should().BeEquivalentTo(apiKey, options => options
            .Excluding(x => x.KeyHash));
    }
}
