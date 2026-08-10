using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevForge.Infrastructure.Persistence;
using DevForge.Domain.Entities;

namespace DevForge.API.Controllers.v1;

/// <summary>
/// Controller for system-level settings and statuses.
/// </summary>
[ApiVersion("1.0")]
public class SystemController : ApiControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SystemController> _logger;

    public SystemController(ApplicationDbContext dbContext, ILogger<SystemController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Checks the overall API status and verifies database read/write capability.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        _logger.LogInformation("System status endpoint invoked.");
        
        try
        {
            var statusLog = new SystemStatusInfo
            {
                CheckedAt = DateTimeOffset.UtcNow
            };

            _dbContext.SystemStatuses.Add(statusLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var count = await _dbContext.SystemStatuses.CountAsync(cancellationToken);

            return Ok(new
            {
                status = "ok",
                application = "DevForge",
                version = "1.0.0",
                databaseConnection = "Connected",
                totalStatusChecks = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection check failed during system status query.");
            
            return Ok(new
            {
                status = "ok",
                application = "DevForge",
                version = "1.0.0",
                databaseConnection = "Offline",
                databaseError = ex.Message
            });
        }
    }
}
