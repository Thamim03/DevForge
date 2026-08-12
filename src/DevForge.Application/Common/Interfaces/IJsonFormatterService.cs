namespace DevForge.Application.Common.Interfaces;

/// <summary>
/// Interface for JSON validation, formatting, and minification operations.
/// </summary>
public interface IJsonFormatterService
{
    /// <summary>
    /// Validates the structure of a JSON string.
    /// </summary>
    (bool IsValid, string? ErrorMessage) Validate(string json);

    /// <summary>
    /// Beautifies/Formats a JSON string with indentation.
    /// </summary>
    string Format(string json);

    /// <summary>
    /// Minifies a JSON string by removing whitespace.
    /// </summary>
    string Minify(string json);
}
