using Microsoft.AspNetCore.Mvc;
using DevForge.Application.Common.Interfaces;
using DevForge.Application.Common.Models;

namespace DevForge.API.Controllers;

/// <summary>
/// Exposes developer tool endpoints (JSON formatting, JWT decoding).
/// </summary>
[ApiController]
[Route("api/tools")]
public class ToolsApiController : ControllerBase
{
    private readonly IJsonFormatterService _jsonFormatter;
    private readonly IJwtInspectorService _jwtInspector;
    private readonly ILogger<ToolsApiController> _logger;

    public ToolsApiController(
        IJsonFormatterService jsonFormatter, 
        IJwtInspectorService jwtInspector, 
        ILogger<ToolsApiController> logger)
    {
        _jsonFormatter = jsonFormatter;
        _jwtInspector = jwtInspector;
        _logger = logger;
    }

    /// <summary>
    /// Beautifies a raw JSON string.
    /// </summary>
    [HttpPost("json/format")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult FormatJson([FromBody] JsonToolRequest request)
    {
        try
        {
            var formatted = _jsonFormatter.Format(request.Json);
            return Ok(new { formatted });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Compacts a JSON string.
    /// </summary>
    [HttpPost("json/minify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult MinifyJson([FromBody] JsonToolRequest request)
    {
        try
        {
            var minified = _jsonFormatter.Minify(request.Json);
            return Ok(new { minified });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Validates whether a string is syntactically valid JSON.
    /// </summary>
    [HttpPost("json/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ValidateJson([FromBody] JsonToolRequest request)
    {
        var result = _jsonFormatter.Validate(request.Json);
        return Ok(new { isValid = result.IsValid, errorMessage = result.ErrorMessage });
    }

    /// <summary>
    /// Decodes a JWT token safely for inspection.
    /// </summary>
    [HttpPost("jwt/decode")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JwtInspectResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult DecodeJwt([FromBody] JwtDecodeRequest request)
    {
        var result = _jwtInspector.Decode(request.Token);
        if (!result.IsValidFormat)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }
        return Ok(result);
    }
}
