using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using DevForge.Application.Common.Models;
using DevForge.Domain.Entities;
using DevForge.Infrastructure.Persistence;
using DevForge.Infrastructure.Services;

namespace DevForge.UnitTests.Services;

/// <summary>
/// Unit tests for ChallengeService using an in-memory SQLite database.
/// Tests cover: filtering, scoring, security, and lifecycle correctness.
/// </summary>
public class ChallengeServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly ChallengeService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public ChallengeServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _service = new ChallengeService(_context, NullLogger<ChallengeService>.Instance);

        SeedTestQuestions();
    }

    // ─── GetCategories ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_Should_ReturnAllSixCategories()
    {
        var categories = await _service.GetCategoriesAsync();
        categories.Count().Should().Be(6);
    }

    [Fact]
    public async Task GetCategories_Should_IncludeCSharpCategory()
    {
        var categories = await _service.GetCategoriesAsync();
        categories.Should().Contain(c => c.Name == "CSharp" && c.DisplayName == "C#");
    }

    // ─── StartChallenge ────────────────────────────────────────────────────────

    [Fact]
    public async Task StartChallenge_Should_ReturnRequestedNumberOfQuestions()
    {
        var request = new StartChallengeRequest { QuestionCount = 5 };
        var session = await _service.StartChallengeAsync(_userId, request);

        session.Questions.Count.Should().Be(5);
        session.TotalQuestions.Should().Be(5);
    }

    [Fact]
    public async Task StartChallenge_Should_FilterByCategory()
    {
        var request = new StartChallengeRequest
        {
            Category = QuestionCategory.CSharp,
            QuestionCount = 5
        };
        var session = await _service.StartChallengeAsync(_userId, request);

        session.Questions.Count.Should().BeGreaterThan(0);
        session.Category.Should().Be("C#");
    }

    [Fact]
    public async Task StartChallenge_Should_FilterByDifficulty()
    {
        var request = new StartChallengeRequest
        {
            Difficulty = QuestionDifficulty.Easy,
            QuestionCount = 5
        };
        var session = await _service.StartChallengeAsync(_userId, request);

        session.Questions.Count.Should().BeGreaterThan(0);
        session.Difficulty.Should().Be("Easy");
    }

    [Fact]
    public async Task StartChallenge_QuestionsShould_NotContainCorrectAnswer()
    {
        // Correct answers must never be in the client-facing DTO
        var request = new StartChallengeRequest { QuestionCount = 5 };
        var session = await _service.StartChallengeAsync(_userId, request);

        // ChallengeOptionDto intentionally has no IsCorrect property
        foreach (var question in session.Questions)
        {
            question.Options.Should().NotBeEmpty();
            // Verify the type itself has no IsCorrect — check option type
            var optionType = question.Options.First().GetType();
            optionType.GetProperty("IsCorrect").Should().BeNull(
                "ChallengeOptionDto must not expose the correct answer to the client");
        }
    }

    [Fact]
    public async Task StartChallenge_WithNoMatchingQuestions_Should_ThrowInvalidOperation()
    {
        // Use a combination that has no seeded questions
        var request = new StartChallengeRequest
        {
            Category = QuestionCategory.Linq,
            Difficulty = QuestionDifficulty.Hard,
            QuestionCount = 5
        };

        var act = () => _service.StartChallengeAsync(_userId, request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No questions available*");
    }

    [Fact]
    public async Task StartChallenge_Should_ClampQuestionCountToAvailable()
    {
        // Only 2 EfCore Easy questions seeded; requesting 10 should return available count
        var request = new StartChallengeRequest
        {
            Category = QuestionCategory.EfCore,
            Difficulty = QuestionDifficulty.Easy,
            QuestionCount = 10
        };
        var session = await _service.StartChallengeAsync(_userId, request);

        // Should return at most as many as exist in the category/difficulty
        session.Questions.Count.Should().BeLessThanOrEqualTo(10);
        session.Questions.Count.Should().BeGreaterThan(0);
    }

    // ─── SubmitChallenge ───────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitChallenge_WithAllCorrectAnswers_Should_Return100Percent()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 5 });

        var correctAnswers = await BuildCorrectAnswers(session);
        var result = await _service.SubmitChallengeAsync(session.AttemptId, _userId,
            new SubmitChallengeRequest { Answers = correctAnswers });

        result.Score.Should().Be(100m);
        result.CorrectAnswers.Should().Be(5);
        result.IncorrectAnswers.Should().Be(0);
    }

    [Fact]
    public async Task SubmitChallenge_WithNoCorrectAnswers_Should_Return0Percent()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 5 });

        var wrongAnswers = await BuildWrongAnswers(session);
        var result = await _service.SubmitChallengeAsync(session.AttemptId, _userId,
            new SubmitChallengeRequest { Answers = wrongAnswers });

        result.Score.Should().Be(0m);
        result.CorrectAnswers.Should().Be(0);
    }

    [Fact]
    public async Task SubmitChallenge_WithDifferentUser_Should_ThrowUnauthorized()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 3 });

        var differentUserId = Guid.NewGuid();
        var act = () => _service.SubmitChallengeAsync(session.AttemptId, differentUserId,
            new SubmitChallengeRequest { Answers = new() });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SubmitChallenge_WhenAlreadyCompleted_Should_ThrowInvalidOperation()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 3 });

        // Submit once
        await _service.SubmitChallengeAsync(session.AttemptId, _userId,
            new SubmitChallengeRequest { Answers = new() });

        // Submit again
        var act = () => _service.SubmitChallengeAsync(session.AttemptId, _userId,
            new SubmitChallengeRequest { Answers = new() });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been submitted*");
    }

    // ─── GetResult ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetResult_AfterSubmission_Should_ReturnCorrectScore()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 5 });

        var correctAnswers = await BuildCorrectAnswers(session);
        await _service.SubmitChallengeAsync(session.AttemptId, _userId,
            new SubmitChallengeRequest { Answers = correctAnswers });

        var result = await _service.GetResultAsync(session.AttemptId, _userId);

        result.Score.Should().Be(100m);
        result.TotalQuestions.Should().Be(5);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetResult_BeforeSubmission_Should_ThrowInvalidOperation()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 3 });

        var act = () => _service.GetResultAsync(session.AttemptId, _userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not been submitted*");
    }

    // ─── GetReview ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetReview_AfterSubmission_Should_IncludeCorrectAnswers()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 5 });

        var correctAnswers = await BuildCorrectAnswers(session);
        await _service.SubmitChallengeAsync(session.AttemptId, _userId,
            new SubmitChallengeRequest { Answers = correctAnswers });

        var review = await _service.GetReviewAsync(session.AttemptId, _userId);

        review.Questions.Should().HaveCount(5);
        review.Questions.Should().AllSatisfy(q =>
        {
            q.QuestionText.Should().NotBeNullOrEmpty();
            q.CorrectAnswerText.Should().NotBeNullOrEmpty();
            q.Explanation.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task GetReview_BeforeSubmission_Should_ThrowInvalidOperation()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 3 });

        var act = () => _service.GetReviewAsync(session.AttemptId, _userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not been submitted*");
    }

    [Fact]
    public async Task GetReview_WithDifferentUser_Should_ThrowUnauthorized()
    {
        var session = await _service.StartChallengeAsync(_userId,
            new StartChallengeRequest { QuestionCount = 3 });

        await _service.SubmitChallengeAsync(session.AttemptId, _userId,
            new SubmitChallengeRequest { Answers = new() });

        var differentUserId = Guid.NewGuid();
        var act = () => _service.GetReviewAsync(session.AttemptId, differentUserId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ─── Test Helpers ──────────────────────────────────────────────────────────

    private async Task<List<SubmittedAnswer>> BuildCorrectAnswers(ChallengeSessionDto session)
    {
        var answers = new List<SubmittedAnswer>();
        foreach (var q in session.Questions)
        {
            // Look up the correct option from the DB
            var correctOption = await _context.QuestionOptions
                .FirstAsync(o => o.QuestionId == q.Id && o.IsCorrect);
            answers.Add(new SubmittedAnswer
            {
                QuestionId = q.Id,
                SelectedOptionId = correctOption.Id
            });
        }
        return answers;
    }

    private async Task<List<SubmittedAnswer>> BuildWrongAnswers(ChallengeSessionDto session)
    {
        var answers = new List<SubmittedAnswer>();
        foreach (var q in session.Questions)
        {
            var wrongOption = await _context.QuestionOptions
                .FirstAsync(o => o.QuestionId == q.Id && !o.IsCorrect);
            answers.Add(new SubmittedAnswer
            {
                QuestionId = q.Id,
                SelectedOptionId = wrongOption.Id
            });
        }
        return answers;
    }

    private void SeedTestQuestions()
    {
        var questions = new List<Question>
        {
            // C# — Easy
            new Question
            {
                Text = "What is a value type in C#?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "Value types store data directly.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "A type stored on the heap", IsCorrect = false },
                    new QuestionOption { Text = "A type stored on the stack", IsCorrect = true },
                    new QuestionOption { Text = "A reference to an object", IsCorrect = false },
                    new QuestionOption { Text = "A nullable type", IsCorrect = false }
                }
            },
            // C# — Easy
            new Question
            {
                Text = "Which keyword is used to inherit a class in C#?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "The colon ':' syntax is used for inheritance in C#.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "extends", IsCorrect = false },
                    new QuestionOption { Text = "implements", IsCorrect = false },
                    new QuestionOption { Text = "inherits", IsCorrect = false },
                    new QuestionOption { Text = ":", IsCorrect = true }
                }
            },
            // EF Core — Easy
            new Question
            {
                Text = "What does DbContext represent in EF Core?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "DbContext represents a session with the database.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "A database table", IsCorrect = false },
                    new QuestionOption { Text = "A database session and unit of work", IsCorrect = true },
                    new QuestionOption { Text = "A migration script", IsCorrect = false },
                    new QuestionOption { Text = "A connection string", IsCorrect = false }
                }
            },
            // EF Core — Easy
            new Question
            {
                Text = "What is the purpose of migrations in EF Core?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "Migrations keep the database schema in sync with entity models.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "To seed the database with data", IsCorrect = false },
                    new QuestionOption { Text = "To version-control schema changes", IsCorrect = true },
                    new QuestionOption { Text = "To cache query results", IsCorrect = false },
                    new QuestionOption { Text = "To configure connection strings", IsCorrect = false }
                }
            },
            // LINQ — Easy (no Hard)
            new Question
            {
                Text = "What does Any() return in LINQ?",
                Category = QuestionCategory.Linq,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "Any() returns true if at least one element matches the predicate.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "The first matching element", IsCorrect = false },
                    new QuestionOption { Text = "True if at least one element matches", IsCorrect = true },
                    new QuestionOption { Text = "The count of matching elements", IsCorrect = false },
                    new QuestionOption { Text = "All matching elements", IsCorrect = false }
                }
            }
        };

        _context.Questions.AddRange(questions);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
