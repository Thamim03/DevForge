using System.Text;
using System.Text.Json;
using DevForge.Application.Common.Interfaces;

namespace DevForge.Application.Services;

/// <summary>
/// Service to decode and inspect JWT tokens safely without signature verification.
/// </summary>
public class JwtInspectorService : IJwtInspectorService
{
    public JwtInspectResult Decode(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new JwtInspectResult
            {
                IsValidFormat = false,
                ErrorMessage = "Token is empty."
            };
        }

        var parts = token.Trim().Split('.');
        if (parts.Length != 3)
        {
            return new JwtInspectResult
            {
                IsValidFormat = false,
                ErrorMessage = "Invalid JWT structure. A JWT must consist of three parts separated by dots."
            };
        }

        try
        {
            // Decode Header
            string headerJson = DecodeBase64Url(parts[0]);
            var formattedHeader = FormatJson(headerJson);

            // Decode Payload
            string payloadJson = DecodeBase64Url(parts[1]);
            var formattedPayload = FormatJson(payloadJson);

            // Parse objects to extract properties
            using var headerDoc = JsonDocument.Parse(headerJson);
            using var payloadDoc = JsonDocument.Parse(payloadJson);

            // Extract Algorithm
            string alg = string.Empty;
            if (headerDoc.RootElement.TryGetProperty("alg", out var algProp))
            {
                alg = algProp.GetString() ?? string.Empty;
            }

            // Extract Claims and Expiration
            var claims = new Dictionary<string, string>();
            string expStatus = "No expiration";
            DateTime? expTime = null;

            foreach (var prop in payloadDoc.RootElement.EnumerateObject())
            {
                string valueStr = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                    JsonValueKind.Number => prop.Value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "null",
                    _ => prop.Value.GetRawText()
                };

                claims[prop.Name] = valueStr;

                // Handle expiration claim
                if (prop.Name == "exp")
                {
                    if (prop.Value.TryGetInt64(out long expUnix))
                    {
                        var utcTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                        expTime = utcTime;
                        expStatus = utcTime < DateTime.UtcNow ? "Expired" : "Valid";
                    }
                }
            }

            return new JwtInspectResult
            {
                IsValidFormat = true,
                HeaderJson = formattedHeader,
                PayloadJson = formattedPayload,
                Algorithm = alg,
                Claims = claims,
                ExpirationStatus = expStatus
            };
        }
        catch (FormatException)
        {
            return new JwtInspectResult
            {
                IsValidFormat = false,
                ErrorMessage = "Invalid Base64URL encoding in token parts."
            };
        }
        catch (JsonException)
        {
            return new JwtInspectResult
            {
                IsValidFormat = false,
                ErrorMessage = "Token contains invalid JSON payload."
            };
        }
        catch (Exception ex)
        {
            return new JwtInspectResult
            {
                IsValidFormat = false,
                ErrorMessage = $"Failed to parse JWT: {ex.Message}"
            };
        }
    }

    private static string DecodeBase64Url(string base64Url)
    {
        string base64 = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        byte[] bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string FormatJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    }
}
