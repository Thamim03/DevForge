using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// Domain entity representing a single answer option for a question.
/// IsCorrect is never sent to the client before the challenge is submitted.
/// </summary>
public class QuestionOption : Entity
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Navigation property for EF Core
    public Question Question { get; set; } = null!;
}
