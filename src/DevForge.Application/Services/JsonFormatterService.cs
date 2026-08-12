using System.Text.Json;
using DevForge.Application.Common.Interfaces;

namespace DevForge.Application.Services;

/// <summary>
/// Implementation of IJsonFormatterService handling validation, formatting, and minification.
/// </summary>
public class JsonFormatterService : IJsonFormatterService
{
    public (bool IsValid, string? ErrorMessage) Validate(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return (false, "JSON input cannot be empty.");
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return (true, null);
        }
        catch (JsonException ex)
        {
            // ex.Message gives a clean message like: "Unexpected character... LineNumber: 0 | BytePositionInLine: 12"
            return (false, ex.Message);
        }
    }

    public string Format(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(doc, options);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", ex);
        }
    }

    public string Minify(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            return JsonSerializer.Serialize(doc, options);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", ex);
        }
    }
}
