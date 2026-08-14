using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevForge.Application.Common.Interfaces;
using DevForge.Application.Common.Models;

namespace DevForge.API.Controllers;

/// <summary>
/// Exposes developer tool endpoints (JSON formatting, JWT decoding, SQL formatting, HTTP Playground).
/// </summary>
[ApiController]
[Route("api/tools")]
public class ToolsApiController : ControllerBase
{
    private readonly IJsonFormatterService _jsonFormatter;
    private readonly IJwtInspectorService _jwtInspector;
    private readonly ISqlFormatterService _sqlFormatter;
    private readonly IApiPlaygroundService _apiPlayground;
    private readonly ILogger<ToolsApiController> _logger;

    public ToolsApiController(
        IJsonFormatterService jsonFormatter, 
        IJwtInspectorService jwtInspector, 
        ISqlFormatterService sqlFormatter,
        IApiPlaygroundService apiPlayground,
        ILogger<ToolsApiController> logger)
    {
        _jsonFormatter = jsonFormatter;
        _jwtInspector = jwtInspector;
        _sqlFormatter = sqlFormatter;
        _apiPlayground = apiPlayground;
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

    /// <summary>
    /// Formats a raw T-SQL string.
    /// </summary>
    [HttpPost("sql/format")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SqlFormatterResponse))]
    public IActionResult FormatSql([FromBody] SqlFormatterRequest request)
    {
        var result = _sqlFormatter.Format(request);
        return Ok(result);
    }

    /// <summary>
    /// Minifies a SQL string by stripping whitespace and comments.
    /// </summary>
    [HttpPost("sql/minify")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SqlFormatterResponse))]
    public IActionResult MinifySql([FromBody] SqlFormatterRequest request)
    {
        var result = _sqlFormatter.Minify(request);
        return Ok(result);
    }

    /// <summary>
    /// Executes an outbound HTTP request inside the API Playground (Requires authentication).
    /// </summary>
    [Authorize]
    [HttpPost("http/request")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiPlaygroundResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExecuteHttpRequest([FromBody] ApiPlaygroundRequest request, CancellationToken cancellationToken)
    {
        var result = await _apiPlayground.SendRequestAsync(request, cancellationToken);
        return Ok(result);
    }
}
