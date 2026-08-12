using System.Text;
using FluentAssertions;
using DevForge.Application.Services;

namespace DevForge.UnitTests.Services;

public class JwtInspectorServiceTests
{
    private readonly JwtInspectorService _service;

    public JwtInspectorServiceTests()
    {
        _service = new JwtInspectorService();
    }

    private string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes)
            .Split('=')[0] // Remove padding
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private string CreateToken(string header, string payload, string signature = "sig")
    {
        return $"{Base64UrlEncode(header)}.{Base64UrlEncode(payload)}.{signature}";
    }

    [Fact]
    public void Decode_WithValidToken_Should_DecodeSuccessfully()
    {
        // Arrange
        var header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
        var payload = "{\"sub\":\"12345\",\"role\":\"User\",\"exp\":4102444800}"; // exp in year 2100
        var token = CreateToken(header, payload);

        // Act
        var result = _service.Decode(token);

        // Assert
        result.IsValidFormat.Should().BeTrue();
        result.Algorithm.Should().Be("HS256");
        result.Claims.Should().ContainKey("sub").WhoseValue.Should().Be("12345");
        result.Claims.Should().ContainKey("role").WhoseValue.Should().Be("User");
        result.ExpirationStatus.Should().Be("Valid");
    }

    [Fact]
    public void Decode_WithExpiredToken_Should_SetStatusToExpired()
    {
        // Arrange
        var header = "{\"alg\":\"HS256\"}";
        var payload = "{\"sub\":\"123\",\"exp\":946684800}"; // exp in year 2000 (expired)
        var token = CreateToken(header, payload);

        // Act
        var result = _service.Decode(token);

        // Assert
        result.IsValidFormat.Should().BeTrue();
        result.ExpirationStatus.Should().Be("Expired");
    }

    [Fact]
    public void Decode_WithNoExpiration_Should_SetStatusToNoExpiration()
    {
        // Arrange
        var header = "{\"alg\":\"HS256\"}";
        var payload = "{\"sub\":\"123\"}"; // No exp claim
        var token = CreateToken(header, payload);

        // Act
        var result = _service.Decode(token);

        // Assert
        result.IsValidFormat.Should().BeTrue();
        result.ExpirationStatus.Should().Be("No expiration");
    }

    [Fact]
    public void Decode_WithInvalidStructure_Should_ReturnInvalidFormat()
    {
        // Act
        var result = _service.Decode("invalid.structure"); // Missing third part

        // Assert
        result.IsValidFormat.Should().BeFalse();
        result.ErrorMessage.Should().Contain("structure");
    }

    [Fact]
    public void Decode_WithInvalidBase64_Should_ReturnInvalidFormat()
    {
        // Arrange
        var token = "not_base64_url!.header.signature";

        // Act
        var result = _service.Decode(token);

        // Assert
        result.IsValidFormat.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Base64URL");
    }

    [Fact]
    public void Decode_WithInvalidJsonPayload_Should_ReturnInvalidFormat()
    {
        // Arrange
        var header = "{\"alg\":\"HS256\"}";
        var payload = "invalid_json_content";
        var token = CreateToken(header, payload);

        // Act
        var result = _service.Decode(token);

        // Assert
        result.IsValidFormat.Should().BeFalse();
        result.ErrorMessage.Should().Contain("JSON");
    }
}
