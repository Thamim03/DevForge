using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// Domain entity representing a single interview question.
/// </summary>
public class Question : Entity
{
    public string Text { get; set; } = string.Empty;
    public QuestionCategory Category { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public string Explanation { get; set; } = string.Empty;

    // Navigation property for EF Core
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}
