using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.Application.Common.Interfaces;
using DevForge.Application.Common.Models;
using DevForge.Domain.Entities;
using DevForge.Infrastructure.Persistence;

namespace DevForge.Infrastructure.Services;

/// <summary>
/// Service implementing the .NET Interview Challenge business logic.
/// All scoring is done server-side. Correct answers are never sent to the client
/// before the challenge is submitted.
/// </summary>
public class ChallengeService : IChallengeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChallengeService> _logger;

    private const int MinQuestions = 5;
    private const int MaxQuestions = 20;

    public ChallengeService(ApplicationDbContext context, ILogger<ChallengeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<IEnumerable<CategoryInfo>> GetCategoriesAsync()
    {
        var categories = Enum.GetValues<QuestionCategory>()
            .Select(c => new CategoryInfo
            {
                Value = (int)c,
                Name = c.ToString(),
                DisplayName = GetCategoryDisplayName(c)
            });

        return Task.FromResult(categories);
    }

    public async Task<ChallengeSessionDto> StartChallengeAsync(
        Guid userId,
        StartChallengeRequest request,
        CancellationToken cancellationToken = default)
    {
        var questionCount = Math.Clamp(request.QuestionCount, MinQuestions, MaxQuestions);

        var query = _context.Questions
            .Include(q => q.Options)
            .AsNoTracking()
            .AsQueryable();

        if (request.Category.HasValue)
        {
            query = query.Where(q => q.Category == request.Category.Value);
        }

        if (request.Difficulty.HasValue)
        {
            query = query.Where(q => q.Difficulty == request.Difficulty.Value);
        }

        // Fetch candidates from the database, then randomly select in memory.
        // This approach is compatible with both SQLite (tests) and SQL Server (production).
        var candidateQuestions = await query
            .ToListAsync(cancellationToken);

        var selectedQuestions = candidateQuestions
            .OrderBy(_ => Guid.NewGuid())
            .Take(questionCount)
            .ToList();

        if (selectedQuestions.Count == 0)
        {
            throw new InvalidOperationException("No questions available for the selected category and difficulty. Try different settings.");
        }

        var attempt = new ChallengeAttempt
        {
            UserId = userId,
            Category = request.Category,
            Difficulty = request.Difficulty,
            TotalQuestions = selectedQuestions.Count,
            StartedAt = DateTimeOffset.UtcNow
        };

        _context.ChallengeAttempts.Add(attempt);

        // Store question IDs in order as answers (initially without a selected option)
        // We track which questions belong to this attempt through ChallengeAnswers with empty SelectedOptionId
        // This approach keeps it simple without a separate join table for attempt-questions.
        // We use a marker: SelectedOptionId = Guid.Empty means "not yet answered"
        foreach (var question in selectedQuestions)
        {
            _context.ChallengeAnswers.Add(new ChallengeAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = question.Id,
                SelectedOptionId = Guid.Empty,
                IsCorrect = false
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Challenge started for user {UserId}, AttemptId: {AttemptId}", userId, attempt.Id);

        return BuildSessionDto(attempt, selectedQuestions);
    }

    public async Task<ChallengeSessionDto> GetChallengeAsync(
        Guid attemptId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var attempt = await _context.ChallengeAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Challenge attempt {attemptId} not found.");

        if (attempt.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have access to this challenge.");
        }

        if (attempt.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("This challenge has already been completed.");
        }

        // Retrieve the questions associated with this attempt
        var questionIds = await _context.ChallengeAnswers
            .AsNoTracking()
            .Where(a => a.AttemptId == attemptId)
            .Select(a => a.QuestionId)
            .ToListAsync(cancellationToken);

        var questions = await _context.Questions
            .Include(q => q.Options)
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        return BuildSessionDto(attempt, questions);
    }

    public async Task<ChallengeResultDto> SubmitChallengeAsync(
        Guid attemptId,
        Guid userId,
        SubmitChallengeRequest request,
        CancellationToken cancellationToken = default)
    {
        var attempt = await _context.ChallengeAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Challenge attempt {attemptId} not found.");

        if (attempt.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have access to this challenge.");
        }

        if (attempt.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("This challenge has already been submitted.");
        }

        // Score each answer server-side — never trust the client
        int correct = 0;
        foreach (var submitted in request.Answers)
        {
            var answerRecord = attempt.Answers
                .FirstOrDefault(a => a.QuestionId == submitted.QuestionId);

            if (answerRecord == null) continue;

            // Look up whether the selected option is correct from the database
            var isCorrect = await _context.QuestionOptions
                .AsNoTracking()
                .AnyAsync(
                    o => o.Id == submitted.SelectedOptionId
                         && o.QuestionId == submitted.QuestionId
                         && o.IsCorrect,
                    cancellationToken);

            answerRecord.SelectedOptionId = submitted.SelectedOptionId;
            answerRecord.IsCorrect = isCorrect;

            if (isCorrect) correct++;
        }

        var percentage = attempt.TotalQuestions > 0
            ? Math.Round((decimal)correct / attempt.TotalQuestions * 100, 2)
            : 0m;

        attempt.CorrectAnswers = correct;
        attempt.Score = percentage;
        attempt.CompletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Challenge {AttemptId} submitted by user {UserId}. Score: {Score}%",
            attemptId, userId, percentage);

        return new ChallengeResultDto
        {
            AttemptId = attempt.Id,
            TotalQuestions = attempt.TotalQuestions,
            CorrectAnswers = correct,
            IncorrectAnswers = attempt.TotalQuestions - correct,
            Score = percentage,
            CompletedAt = attempt.CompletedAt
        };
    }

    public async Task<ChallengeResultDto> GetResultAsync(
        Guid attemptId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var attempt = await _context.ChallengeAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Challenge attempt {attemptId} not found.");

        if (attempt.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have access to this challenge.");
        }

        if (!attempt.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("This challenge has not been submitted yet.");
        }

        return new ChallengeResultDto
        {
            AttemptId = attempt.Id,
            TotalQuestions = attempt.TotalQuestions,
            CorrectAnswers = attempt.CorrectAnswers,
            IncorrectAnswers = attempt.TotalQuestions - attempt.CorrectAnswers,
            Score = attempt.Score,
            CompletedAt = attempt.CompletedAt
        };
    }

    public async Task<ChallengeReviewDto> GetReviewAsync(
        Guid attemptId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var attempt = await _context.ChallengeAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Challenge attempt {attemptId} not found.");

        if (attempt.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have access to this challenge.");
        }

        if (!attempt.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("This challenge has not been submitted yet.");
        }

        var answers = await _context.ChallengeAnswers
            .AsNoTracking()
            .Where(a => a.AttemptId == attemptId)
            .ToListAsync(cancellationToken);

        var questionIds = answers.Select(a => a.QuestionId).ToList();

        var questions = await _context.Questions
            .Include(q => q.Options)
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        var reviewQuestions = answers.Select(answer =>
        {
            var question = questions.First(q => q.Id == answer.QuestionId);
            var selectedOption = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId);
            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);

            return new ReviewQuestionDto
            {
                QuestionText = question.Text,
                YourAnswerText = selectedOption?.Text ?? "No answer selected",
                CorrectAnswerText = correctOption?.Text ?? "N/A",
                IsCorrect = answer.IsCorrect,
                Explanation = question.Explanation
            };
        }).ToList();

        return new ChallengeReviewDto
        {
            AttemptId = attempt.Id,
            TotalQuestions = attempt.TotalQuestions,
            CorrectAnswers = attempt.CorrectAnswers,
            Score = attempt.Score,
            Questions = reviewQuestions
        };
    }

    // ─── Private Helpers ───────────────────────────────────────────────────────

    private static ChallengeSessionDto BuildSessionDto(ChallengeAttempt attempt, List<Question> questions)
    {
        return new ChallengeSessionDto
        {
            AttemptId = attempt.Id,
            Category = attempt.Category.HasValue
                ? GetCategoryDisplayName(attempt.Category.Value)
                : null,
            Difficulty = attempt.Difficulty?.ToString(),
            TotalQuestions = questions.Count,
            Questions = questions.Select(q => new ChallengeQuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                // IsCorrect is intentionally excluded from ChallengeOptionDto
                Options = q.Options
                    .Select(o => new ChallengeOptionDto { Id = o.Id, Text = o.Text })
                    .ToList()
            }).ToList()
        };
    }

    private static string GetCategoryDisplayName(QuestionCategory category) => category switch
    {
        QuestionCategory.CSharp => "C#",
        QuestionCategory.AspNetCore => "ASP.NET Core",
        QuestionCategory.WebApi => "Web API",
        QuestionCategory.EfCore => "EF Core",
        QuestionCategory.SqlServer => "SQL Server",
        QuestionCategory.Linq => "LINQ",
        _ => category.ToString()
    };
}
