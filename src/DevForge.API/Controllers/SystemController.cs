using Microsoft.AspNetCore.Mvc;

namespace DevForge.API.Controllers;

/// <summary>
/// Controller for system-level settings and statuses.
/// </summary>
[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly ILogger<SystemController> _logger;

    public SystemController(ILogger<SystemController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Checks the overall API status.
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("System status endpoint invoked.");
        
        return Ok(new
        {
            application = "DevForge",
            status = "Healthy"
        });
    }
}
