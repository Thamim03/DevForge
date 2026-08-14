using System.Collections.Generic;

namespace DevForge.Application.Common.Models;

/// <summary>
/// Request payload for the API Playground request execution.
/// </summary>
public class ApiPlaygroundRequest
{
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, string>? QueryParameters { get; set; }
    public string? Body { get; set; }
}

/// <summary>
/// Response returned to the frontend showing status, time, response headers, and response body.
/// </summary>
public class ApiPlaygroundResponse
{
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
    public string? ErrorMessage { get; set; }
}
