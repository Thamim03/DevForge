using FluentAssertions;
using DevForge.Application.Common.Models;

namespace DevForge.UnitTests;

/// <summary>
/// Unit tests for the Result and Error patterns.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_Should_CreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_Should_CreateFailureResult_WithError()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void SuccessGeneric_Should_CreateSuccessResultWithValue()
    {
        // Arrange
        var value = "test-value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void FailureGeneric_Should_ThrowException_WhenAccessingValue()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Act & Assert
        var act = () => { var val = result.Value; };
        act.Should().Throw<InvalidOperationException>();
    }
}
