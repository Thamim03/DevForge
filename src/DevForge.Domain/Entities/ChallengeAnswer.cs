using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// Domain entity representing the user's answer to a single question within a challenge attempt.
/// </summary>
public class ChallengeAnswer : Entity
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }

    // Navigation properties for EF Core
    public ChallengeAttempt Attempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
