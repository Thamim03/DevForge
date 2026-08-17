using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevForge.Application.Common.Interfaces;
using DevForge.Application.Common.Models;

namespace DevForge.API.Controllers;

/// <summary>
/// Handles .NET Interview Challenge endpoints.
/// Correct answers are never sent to the client before submission.
/// </summary>
[ApiController]
[Route("api/challenges")]
public class ChallengeController : ControllerBase
{
    private readonly IChallengeService _challengeService;
    private readonly ILogger<ChallengeController> _logger;

    public ChallengeController(IChallengeService challengeService, ILogger<ChallengeController> logger)
    {
        _challengeService = challengeService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all available question categories.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryInfo>))]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _challengeService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Starts a new challenge session. Returns questions without correct answers.
    /// </summary>
    [Authorize]
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChallengeSessionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StartChallenge(
        [FromBody] StartChallengeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.QuestionCount < 5 || request.QuestionCount > 20)
        {
            return BadRequest(new { message = "Question count must be between 5 and 20." });
        }

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token claims." });
        }

        try
        {
            var session = await _challengeService.StartChallengeAsync(userId.Value, request, cancellationToken);
            return Ok(session);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves an in-progress challenge session. Returns questions without correct answers.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChallengeSessionDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChallenge(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

        try
        {
            var session = await _challengeService.GetChallengeAsync(id, userId.Value, cancellationToken);
            return Ok(session);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Challenge not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submits all answers for a challenge. Scoring is calculated server-side.
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChallengeResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitChallenge(
        Guid id,
        [FromBody] SubmitChallengeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

        try
        {
            var result = await _challengeService.SubmitChallengeAsync(id, userId.Value, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Challenge not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns the score summary for a completed challenge.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}/result")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChallengeResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResult(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

        try
        {
            var result = await _challengeService.GetResultAsync(id, userId.Value, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Challenge not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Returns the full answer review for a completed challenge, including correct answers and explanations.
    /// Only available after submission.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChallengeReviewDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReview(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

        try
        {
            var review = await _challengeService.GetReviewAsync(id, userId.Value, cancellationToken);
            return Ok(review);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Challenge not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── Private Helpers ───────────────────────────────────────────────────────

    private Guid? GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return null;
        return userId;
    }
}
