using Microsoft.AspNetCore.Mvc;
using DevForge.Application.Common.Models;

namespace DevForge.API.Controllers;

/// <summary>
/// Abstract base controller containing common utilities for endpoints, such as mapping Result to IActionResult.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return MapErrorToActionResult(result.Error);
    }

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapErrorToActionResult(result.Error);
    }

    private ActionResult MapErrorToActionResult(Error error)
    {
        // Simple mapping from Error code to status codes
        var statusCode = error.Code switch
        {
            "Error.NullValue" => StatusCodes.Status400BadRequest,
            "Error.NotFound" => StatusCodes.Status404NotFound,
            "Error.Unauthorized" => StatusCodes.Status401Unauthorized,
            "Error.Forbidden" => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Title = "Request Failed",
            Detail = error.Message,
            Status = statusCode,
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["code"] = error.Code;
        problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;

        if (!string.IsNullOrEmpty(error.Detail))
        {
            problemDetails.Extensions["errorDetail"] = error.Detail;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
