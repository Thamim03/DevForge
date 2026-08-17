using DevForge.Application.Common.Models;

namespace DevForge.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for the .NET Interview Challenge feature.
/// </summary>
public interface IChallengeService
{
    /// <summary>
    /// Returns all available question categories.
    /// </summary>
    Task<IEnumerable<CategoryInfo>> GetCategoriesAsync();

    /// <summary>
    /// Starts a new challenge session for the given user.
    /// Questions are returned WITHOUT the correct answer.
    /// </summary>
    Task<ChallengeSessionDto> StartChallengeAsync(
        Guid userId,
        StartChallengeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an in-progress challenge session.
    /// Throws UnauthorizedAccessException if the attempt does not belong to the user.
    /// </summary>
    Task<ChallengeSessionDto> GetChallengeAsync(
        Guid attemptId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits answers, calculates the score server-side, and marks the attempt as complete.
    /// Throws UnauthorizedAccessException if the attempt does not belong to the user.
    /// Throws InvalidOperationException if the challenge is already completed.
    /// </summary>
    Task<ChallengeResultDto> SubmitChallengeAsync(
        Guid attemptId,
        Guid userId,
        SubmitChallengeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the score summary for a completed challenge.
    /// Throws UnauthorizedAccessException if the attempt does not belong to the user.
    /// </summary>
    Task<ChallengeResultDto> GetResultAsync(
        Guid attemptId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full answer review for a completed challenge.
    /// Only available after submission.
    /// Throws UnauthorizedAccessException if the attempt does not belong to the user.
    /// </summary>
    Task<ChallengeReviewDto> GetReviewAsync(
        Guid attemptId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
