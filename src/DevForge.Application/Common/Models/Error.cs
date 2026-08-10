namespace DevForge.Application.Common.Models;

/// <summary>
/// Encapsulates error information for responses.
/// </summary>
public record Error(string Code, string Message, string? Detail = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified value is null.");
}
