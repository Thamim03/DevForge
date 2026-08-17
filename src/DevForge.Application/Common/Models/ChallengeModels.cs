using DevForge.Domain.Entities;

namespace DevForge.Application.Common.Models;

// ─── Request Models ────────────────────────────────────────────────────────

/// <summary>
/// Request to start a new challenge session.
/// </summary>
public class StartChallengeRequest
{
    /// <summary>
    /// Optional category filter. Null means all categories.
    /// </summary>
    public QuestionCategory? Category { get; set; }

    /// <summary>
    /// Optional difficulty filter. Null means all difficulties.
    /// </summary>
    public QuestionDifficulty? Difficulty { get; set; }

    /// <summary>
    /// Number of questions to include. Must be between 5 and 20.
    /// </summary>
    public int QuestionCount { get; set; } = 10;
}

/// <summary>
/// A single answer submitted when completing a challenge.
/// </summary>
public class SubmittedAnswer
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}

/// <summary>
/// Request containing all answers for challenge submission.
/// </summary>
public class SubmitChallengeRequest
{
    public List<SubmittedAnswer> Answers { get; set; } = new();
}

// ─── Response / DTO Models ─────────────────────────────────────────────────

/// <summary>
/// A single answer option sent to the client.
/// IsCorrect is intentionally excluded — it is determined server-side.
/// </summary>
public class ChallengeOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// A single question sent to the client during a challenge.
/// Does NOT include the correct answer.
/// </summary>
public class ChallengeQuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<ChallengeOptionDto> Options { get; set; } = new();
}

/// <summary>
/// Returned when a challenge session is started or retrieved.
/// </summary>
public class ChallengeSessionDto
{
    public Guid AttemptId { get; set; }
    public string? Category { get; set; }
    public string? Difficulty { get; set; }
    public int TotalQuestions { get; set; }
    public List<ChallengeQuestionDto> Questions { get; set; } = new();
}

/// <summary>
/// Returned after a challenge is submitted — score summary only.
/// </summary>
public class ChallengeResultDto
{
    public Guid AttemptId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public decimal Score { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

/// <summary>
/// A single reviewed question shown after challenge completion.
/// </summary>
public class ReviewQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string YourAnswerText { get; set; } = string.Empty;
    public string CorrectAnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

/// <summary>
/// Full review of a completed challenge with correct answers and explanations.
/// </summary>
public class ChallengeReviewDto
{
    public Guid AttemptId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal Score { get; set; }
    public List<ReviewQuestionDto> Questions { get; set; } = new();
}

/// <summary>
/// Category information returned for the challenge setup screen.
/// </summary>
public class CategoryInfo
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
