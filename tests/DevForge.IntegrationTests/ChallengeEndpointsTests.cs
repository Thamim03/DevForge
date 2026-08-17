using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using DevForge.Application.Common.Models;
using DevForge.Domain.Entities;
using DevForge.Infrastructure.Persistence;

namespace DevForge.IntegrationTests;

/// <summary>
/// Integration tests for the /api/challenges/* endpoints.
/// Uses an in-memory SQLite database with seeded questions.
/// </summary>
public class ChallengeEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _sqliteConnection;

    public ChallengeEndpointsTests()
    {
        _sqliteConnection = new SqliteConnection("Data Source=ChallengeIntTestDb;Mode=Memory;Cache=Shared");
        _sqliteConnection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Data Source=ChallengeIntTestDb;Mode=Memory;Cache=Shared");
        });

        // Ensure schema and seeded questions are ready
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        // Run the seeder to populate question bank
        var initializer = scope.ServiceProvider.GetRequiredService<DbContextInitializer>();
        initializer.SeedAsync().GetAwaiter().GetResult();
    }

    // ─── Categories ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_Should_ReturnAllSixCategories()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/challenges/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryInfo>>();
        categories.Should().NotBeNull();
        categories!.Count().Should().Be(6);
    }

    // ─── Start Challenge ───────────────────────────────────────────────────────

    [Fact]
    public async Task StartChallenge_WithoutAuth_Should_Return401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartChallenge_WithValidAuth_Should_ReturnSessionWithQuestions()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<ChallengeSessionDto>();
        session.Should().NotBeNull();
        session!.AttemptId.Should().NotBeEmpty();
        session.Questions.Should().HaveCount(5);
        session.TotalQuestions.Should().Be(5);
    }

    [Fact]
    public async Task StartChallenge_Questions_Should_NotContainIsCorrectField()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<ChallengeSessionDto>();
        session.Should().NotBeNull();

        // The raw JSON should NOT contain "isCorrect" anywhere
        var rawJson = await response.Content.ReadAsStringAsync();
        rawJson.Should().NotContain("isCorrect",
            "correct answers must never be sent to the client before submission");
    }

    [Fact]
    public async Task StartChallenge_WithInvalidQuestionCount_Should_Return400()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 100 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StartChallenge_WithCategoryFilter_Should_ReturnFilteredSession()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest
            {
                Category = QuestionCategory.CSharp,
                QuestionCount = 5
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<ChallengeSessionDto>();
        session.Should().NotBeNull();
        session!.Category.Should().Be("C#");
    }

    // ─── Get Challenge ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetChallenge_WithoutAuth_Should_Return401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/challenges/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChallenge_WithNonExistentId_Should_Return404()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetAsync($"/api/challenges/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetChallenge_WithValidId_Should_ReturnSession()
    {
        var client = await GetAuthenticatedClientAsync();

        // Start a challenge first
        var startResponse = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();

        // Get the same challenge
        var getResponse = await client.GetAsync($"/api/challenges/{session!.AttemptId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrieved = await getResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();
        retrieved.Should().NotBeNull();
        retrieved!.AttemptId.Should().Be(session.AttemptId);
    }

    // ─── Submit Challenge ──────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitChallenge_WithoutAuth_Should_Return401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/challenges/{Guid.NewGuid()}/submit",
            new SubmitChallengeRequest());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitChallenge_Should_ReturnResultWithScore()
    {
        var client = await GetAuthenticatedClientAsync();

        var startResponse = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();
        session.Should().NotBeNull();

        // Submit with empty answers (all incorrect/unanswered)
        var submitRequest = new SubmitChallengeRequest { Answers = new List<SubmittedAnswer>() };
        var submitResponse = await client.PostAsJsonAsync(
            $"/api/challenges/{session!.AttemptId}/submit", submitRequest);

        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await submitResponse.Content.ReadFromJsonAsync<ChallengeResultDto>();
        result.Should().NotBeNull();
        result!.AttemptId.Should().Be(session.AttemptId);
        result.TotalQuestions.Should().Be(5);
        result.Score.Should().BeInRange(0m, 100m);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitChallenge_Twice_Should_Return400()
    {
        var client = await GetAuthenticatedClientAsync();

        var startResponse = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();

        var submitRequest = new SubmitChallengeRequest { Answers = new List<SubmittedAnswer>() };

        // First submission
        await client.PostAsJsonAsync($"/api/challenges/{session!.AttemptId}/submit", submitRequest);

        // Second submission should fail
        var secondResponse = await client.PostAsJsonAsync(
            $"/api/challenges/{session.AttemptId}/submit", submitRequest);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── Get Result ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetResult_BeforeSubmission_Should_Return400()
    {
        var client = await GetAuthenticatedClientAsync();

        var startResponse = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();

        var response = await client.GetAsync($"/api/challenges/{session!.AttemptId}/result");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetResult_AfterSubmission_Should_Return200WithScore()
    {
        var client = await GetAuthenticatedClientAsync();

        var startResponse = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();

        await client.PostAsJsonAsync($"/api/challenges/{session!.AttemptId}/submit",
            new SubmitChallengeRequest { Answers = new List<SubmittedAnswer>() });

        var resultResponse = await client.GetAsync($"/api/challenges/{session.AttemptId}/result");
        resultResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await resultResponse.Content.ReadFromJsonAsync<ChallengeResultDto>();
        result.Should().NotBeNull();
        result!.TotalQuestions.Should().Be(5);
    }

    // ─── Get Review ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetReview_BeforeSubmission_Should_Return400()
    {
        var client = await GetAuthenticatedClientAsync();

        var startResponse = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();

        var response = await client.GetAsync($"/api/challenges/{session!.AttemptId}/review");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetReview_AfterSubmission_Should_ReturnQuestionsWithExplanations()
    {
        var client = await GetAuthenticatedClientAsync();

        var startResponse = await client.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();

        await client.PostAsJsonAsync($"/api/challenges/{session!.AttemptId}/submit",
            new SubmitChallengeRequest { Answers = new List<SubmittedAnswer>() });

        var reviewResponse = await client.GetAsync($"/api/challenges/{session.AttemptId}/review");
        reviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var review = await reviewResponse.Content.ReadFromJsonAsync<ChallengeReviewDto>();
        review.Should().NotBeNull();
        review!.Questions.Should().HaveCount(5);
        review.Questions.Should().AllSatisfy(q =>
        {
            q.QuestionText.Should().NotBeNullOrEmpty();
            q.CorrectAnswerText.Should().NotBeNullOrEmpty();
            q.Explanation.Should().NotBeNullOrEmpty();
        });
    }

    // ─── Cross-user Authorization ──────────────────────────────────────────────

    [Fact]
    public async Task SubmitChallenge_ByDifferentUser_Should_Return403()
    {
        // User 1 starts a challenge
        var client1 = await GetAuthenticatedClientAsync("user@devforge.com", "User123!");
        var startResponse = await client1.PostAsJsonAsync("/api/challenges/start",
            new StartChallengeRequest { QuestionCount = 5 });
        var session = await startResponse.Content.ReadFromJsonAsync<ChallengeSessionDto>();

        // User 2 tries to submit it
        var client2 = await GetAuthenticatedClientAsync("admin@devforge.com", "Admin123!");
        var response = await client2.PostAsJsonAsync(
            $"/api/challenges/{session!.AttemptId}/submit",
            new SubmitChallengeRequest { Answers = new List<SubmittedAnswer>() });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ─── Private Helpers ───────────────────────────────────────────────────────

    private async Task<HttpClient> GetAuthenticatedClientAsync(
        string usernameOrEmail = "user@devforge.com",
        string password = "User123!")
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { UsernameOrEmail = usernameOrEmail, Password = password });

        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    public void Dispose()
    {
        _factory.Dispose();
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();
    }
}
