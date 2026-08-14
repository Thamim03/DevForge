using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DevForge.Application.Common.Interfaces;
using DevForge.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace DevForge.Application.Services;

/// <summary>
/// Service executing HTTP requests for the API Playground with built-in SSRF protections.
/// </summary>
public class ApiPlaygroundService : IApiPlaygroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiPlaygroundService> _logger;

    public ApiPlaygroundService(IHttpClientFactory httpClientFactory, ILogger<ApiPlaygroundService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ApiPlaygroundResponse> SendRequestAsync(ApiPlaygroundRequest request, CancellationToken cancellationToken = default)
    {
        var responseDto = new ApiPlaygroundResponse();

        if (request == null)
        {
            responseDto.StatusCode = 400;
            responseDto.ErrorMessage = "Request cannot be null.";
            return responseDto;
        }

        // 1. SSRF and URL Validation
        var (isValidUrl, urlError) = await ValidateUrlAsync(request.Url);
        if (!isValidUrl)
        {
            _logger.LogWarning("Blocked potentially malicious URL request: {Url}", RedactUrl(request.Url));
            responseDto.StatusCode = 400;
            responseDto.ErrorMessage = urlError ?? "Invalid URL.";
            return responseDto;
        }

        var stopwatch = new Stopwatch();

        try
        {
            // 2. Build URL with query parameters
            var finalUrl = BuildUrlWithQueryParameters(request.Url, request.QueryParameters);

            // 3. Create request message
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method.ToUpperInvariant()), finalUrl);

            // 4. Handle Request Body for POST/PUT
            HttpContent? requestContent = null;
            if (string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.Method, "PUT", StringComparison.OrdinalIgnoreCase))
            {
                var bodyText = request.Body ?? string.Empty;
                var contentType = "application/json";

                if (request.Headers != null && request.Headers.TryGetValue("Content-Type", out var contentHeader))
                {
                    contentType = contentHeader;
                }

                // Validate JSON if content type is JSON
                if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(bodyText))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(bodyText);
                    }
                    catch (JsonException ex)
                    {
                        responseDto.StatusCode = 400;
                        responseDto.ErrorMessage = $"Invalid JSON body: {ex.Message}";
                        return responseDto;
                    }
                }

                var mediaType = contentType.Split(';')[0].Trim();
                requestContent = new StringContent(bodyText, Encoding.UTF8, mediaType);
                requestMessage.Content = requestContent;
            }

            // 5. Append Headers
            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    if (string.IsNullOrWhiteSpace(header.Key)) continue;

                    // Content-Type is handled when creating StringContent
                    if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        if (requestMessage.Content != null)
                        {
                            try
                            {
                                requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(header.Value);
                            }
                            catch { }
                        }
                        continue;
                    }

                    if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                    {
                        if (requestMessage.Content != null)
                        {
                            requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    else
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            // 6. Execute Request
            var client = _httpClientFactory.CreateClient("ApiPlaygroundClient");
            client.Timeout = TimeSpan.FromSeconds(10); // 10s request timeout

            _logger.LogInformation("Sending API Playground request: Method={Method}", request.Method);

            stopwatch.Start();
            using var responseMessage = await client.SendAsync(requestMessage, cancellationToken);
            stopwatch.Stop();

            // 7. Parse Response
            responseDto.StatusCode = (int)responseMessage.StatusCode;
            responseDto.StatusDescription = responseMessage.ReasonPhrase ?? responseMessage.StatusCode.ToString();
            responseDto.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            // Load response headers
            foreach (var header in responseMessage.Headers)
            {
                responseDto.Headers[header.Key] = string.Join(", ", header.Value);
            }
            if (responseMessage.Content != null)
            {
                foreach (var header in responseMessage.Content.Headers)
                {
                    responseDto.Headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            // Read response body (safe limit of 2MB to protect server memory)
            const int maxResponseBytes = 2 * 1024 * 1024;
            if (responseMessage.Content != null)
            {
                var responseBytes = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken);
                if (responseBytes.Length > maxResponseBytes)
                {
                    var bodySnippet = Encoding.UTF8.GetString(responseBytes, 0, maxResponseBytes);
                    responseDto.Body = bodySnippet + "\r\n... [Response body truncated. Size exceeds 2MB safety limit] ...";
                }
                else
                {
                    responseDto.Body = Encoding.UTF8.GetString(responseBytes);
                }
            }
            else
            {
                responseDto.Body = string.Empty;
            }
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            if (stopwatch.IsRunning) stopwatch.Stop();
            responseDto.StatusCode = 408;
            responseDto.StatusDescription = "Request Timeout";
            responseDto.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            responseDto.ErrorMessage = "The request timed out. The server did not respond within the 10-second limit.";
        }
        catch (HttpRequestException ex)
        {
            if (stopwatch.IsRunning) stopwatch.Stop();
            responseDto.StatusCode = 0;
            responseDto.StatusDescription = "Network Error";
            responseDto.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            responseDto.ErrorMessage = $"A network or server error occurred: {ex.Message}";
        }
        catch (Exception ex)
        {
            if (stopwatch.IsRunning) stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error in API Playground SendRequestAsync.");
            responseDto.StatusCode = 500;
            responseDto.StatusDescription = "Internal Server Error";
            responseDto.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            responseDto.ErrorMessage = "An unexpected error occurred while processing the request.";
        }

        return responseDto;
    }

    private static string BuildUrlWithQueryParameters(string baseUrl, Dictionary<string, string>? queryParameters)
    {
        var uriBuilder = new UriBuilder(baseUrl);
        var querySegments = new List<string>();

        if (!string.IsNullOrEmpty(uriBuilder.Query))
        {
            var existingQuery = uriBuilder.Query.TrimStart('?');
            if (!string.IsNullOrEmpty(existingQuery))
            {
                querySegments.Add(existingQuery);
            }
        }

        if (queryParameters != null)
        {
            foreach (var param in queryParameters)
            {
                if (string.IsNullOrWhiteSpace(param.Key)) continue;
                querySegments.Add($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value ?? string.Empty)}");
            }
        }

        if (querySegments.Count > 0)
        {
            uriBuilder.Query = string.Join("&", querySegments);
        }

        return uriBuilder.Uri.ToString();
    }

    private static async Task<(bool IsValid, string? ErrorMessage)> ValidateUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return (false, "URL cannot be empty.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return (false, "Invalid URL format.");
        }

        // Only allow HTTP/HTTPS
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return (false, "Only HTTP and HTTPS protocols are supported.");
        }

        var host = uri.Host;

        // Reject immediate loopback and localdomain checks
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("127.0.0.1") ||
            host.Equals("[::1]") ||
            host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Requests to localhost or internal network addresses are not allowed.");
        }

        // Resolve DNS to verify IP ranges (SSRF protection)
        try
        {
            var ipAddresses = await Dns.GetHostAddressesAsync(host);
            if (ipAddresses == null || ipAddresses.Length == 0)
            {
                return (false, "Could not resolve hostname.");
            }

            foreach (var ip in ipAddresses)
            {
                if (IsPrivateOrInternalIp(ip))
                {
                    return (false, "Requests to private or internal IP ranges are not allowed.");
                }
            }
        }
        catch (SocketException)
        {
            // Host could not be resolved, allow HttpClient to handle DNS failures normally
        }
        catch (Exception ex)
        {
            return (false, $"URL validation error: {ex.Message}");
        }

        return (true, null);
    }

    private static bool IsPrivateOrInternalIp(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10) return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168) return true;

            // 169.254.0.0/16 (Link Local)
            if (bytes[0] == 169 && bytes[1] == 254) return true;

            // 0.0.0.0 (Unspecified)
            if (bytes[0] == 0) return true;
        }
        else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal)
            {
                return true;
            }

            var bytes = ip.GetAddressBytes();
            
            // Unique Local Addresses (fc00::/7)
            if ((bytes[0] & 0xFE) == 0xFC) return true;
        }

        return false;
    }

    private static string RedactUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}:{uri.Port}/[redacted]";
        }
        catch
        {
            return "[malformed URL]";
        }
    }
}
