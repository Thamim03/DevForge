using FluentAssertions;
using DevForge.Application.Services;

namespace DevForge.UnitTests.Services;

public class JsonFormatterServiceTests
{
    private readonly JsonFormatterService _service;

    public JsonFormatterServiceTests()
    {
        _service = new JsonFormatterService();
    }

    [Fact]
    public void Validate_WithValidJson_Should_ReturnTrue()
    {
        // Arrange
        var json = "{\"name\":\"DevForge\",\"type\":\"web\"}";

        // Act
        var result = _service.Validate(json);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Validate_WithInvalidJson_Should_ReturnFalseAndErrorMessage()
    {
        // Arrange
        var json = "{\"name\":\"DevForge\", \"type\": }"; // Invalid JSON

        // Act
        var result = _service.Validate(json);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("invalid");
    }

    [Fact]
    public void Validate_WithEmptyInput_Should_ReturnFalse()
    {
        // Act
        var result = _service.Validate("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("empty");
    }

    [Fact]
    public void Format_WithValidJson_Should_ReturnFormattedJson()
    {
        // Arrange
        var json = "{\"name\":\"DevForge\"}";

        // Act
        var formatted = _service.Format(json);

        // Assert
        formatted.Should().Contain("\n");
        formatted.Should().Contain("  \"name\": \"DevForge\"");
    }

    [Fact]
    public void Format_WithInvalidJson_Should_ThrowArgumentException()
    {
        // Arrange
        var json = "{\"name\": ";

        // Act
        var act = () => _service.Format(json);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Minify_WithValidJson_Should_ReturnCompactJson()
    {
        // Arrange
        var json = @"
        {
            ""name"": ""DevForge"",
            ""active"": true
        }";

        // Act
        var minified = _service.Minify(json);

        // Assert
        minified.Should().Be("{\"name\":\"DevForge\",\"active\":true}");
    }

    [Fact]
    public void Minify_WithInvalidJson_Should_ThrowArgumentException()
    {
        // Arrange
        var json = "{ invalid }";

        // Act
        var act = () => _service.Minify(json);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Format_WithNestedJsonAndArrays_Should_FormatSuccessfully()
    {
        // Arrange
        var json = "{\"id\":1,\"tags\":[\"c#\",\"dotnet\"],\"info\":{\"verified\":true}}";

        // Act
        var formatted = _service.Format(json);

        // Assert
        formatted.Should().Contain("  \"id\": 1");
        formatted.Should().Contain("  \"tags\": [");
        formatted.Should().Contain("    \"c#\",");
        formatted.Should().Contain("    \"verified\": true");
    }
}
