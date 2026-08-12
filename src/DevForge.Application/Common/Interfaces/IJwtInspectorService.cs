namespace DevForge.Application.Common.Interfaces;

/// <summary>
/// Data transfer object detailing parsed JWT information.
/// </summary>
public class JwtInspectResult
{
    public bool IsValidFormat { get; set; }
    public string HeaderJson { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public Dictionary<string, string> Claims { get; set; } = new();
    public string ExpirationStatus { get; set; } = string.Empty; // "Valid", "Expired", "No expiration"
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Interface for parsing and inspecting JWT tokens.
/// </summary>
public interface IJwtInspectorService
{
    /// <summary>
    /// Decodes a JWT token safely without checking the signature.
    /// </summary>
    JwtInspectResult Decode(string token);
}
