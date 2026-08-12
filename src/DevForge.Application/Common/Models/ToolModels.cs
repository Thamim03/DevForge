namespace DevForge.Application.Common.Models;

/// <summary>
/// Request payload for JSON formatter tool operations.
/// </summary>
public class JsonToolRequest
{
    public string Json { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for JWT inspection.
/// </summary>
public class JwtDecodeRequest
{
    public string Token { get; set; } = string.Empty;
}
