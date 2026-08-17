using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// Domain entity representing a single challenge session for a user.
/// </summary>
public class ChallengeAttempt : Entity
{
    public Guid UserId { get; set; }
    public QuestionCategory? Category { get; set; }
    public QuestionDifficulty? Difficulty { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal Score { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // Navigation property for EF Core
    public ICollection<ChallengeAnswer> Answers { get; set; } = new List<ChallengeAnswer>();
}
