using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevForge.Application.Common.Models;

namespace DevForge.Web.Controllers;

/// <summary>
/// MVC controller for the .NET Interview Challenge feature.
/// All challenge data operations are delegated to the API via HttpClient.
/// </summary>
[Authorize]
public class ChallengeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChallengeController> _logger;

    public ChallengeController(IHttpClientFactory httpClientFactory, ILogger<ChallengeController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Challenge home page — choose category, difficulty, and question count.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = ".NET Interview Challenge";
        try
        {
            var client = GetApiClient();
            var categories = await client.GetFromJsonAsync<IEnumerable<CategoryInfo>>("/api/challenges/categories");
            ViewBag.Categories = categories ?? Enumerable.Empty<CategoryInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load challenge categories.");
            ViewBag.Categories = Enumerable.Empty<CategoryInfo>();
        }
        return View();
    }

    /// <summary>
    /// Starts a new challenge session and redirects to the question screen.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(StartChallengeRequest request)
    {
        try
        {
            var client = GetApiClient();
            AttachBearerToken(client);

            var response = await client.PostAsJsonAsync("/api/challenges/start", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                TempData["ErrorMessage"] = error?.Message ?? "Failed to start the challenge. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            var session = await response.Content.ReadFromJsonAsync<ChallengeSessionDto>();
            if (session == null)
            {
                TempData["ErrorMessage"] = "Failed to start the challenge. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Take), new { id = session.AttemptId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting challenge.");
            TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Displays the question screen for an in-progress challenge.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Take(Guid id)
    {
        ViewData["Title"] = ".NET Interview Challenge";
        try
        {
            var client = GetApiClient();
            AttachBearerToken(client);

            var response = await client.GetAsync($"/api/challenges/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Challenge not found or already completed.";
                return RedirectToAction(nameof(Index));
            }

            var session = await response.Content.ReadFromJsonAsync<ChallengeSessionDto>();
            if (session == null)
            {
                TempData["ErrorMessage"] = "Failed to load the challenge. Please start a new one.";
                return RedirectToAction(nameof(Index));
            }

            return View(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading challenge {AttemptId}.", id);
            TempData["ErrorMessage"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Submits answers for a challenge and redirects to the result page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Guid id, [FromForm] Dictionary<string, string> answers)
    {
        try
        {
            var client = GetApiClient();
            AttachBearerToken(client);

            // Build the submit request from the form data (questionId -> selectedOptionId)
            var submittedAnswers = answers
                .Where(kvp => Guid.TryParse(kvp.Key, out _) && Guid.TryParse(kvp.Value, out _))
                .Select(kvp => new SubmittedAnswer
                {
                    QuestionId = Guid.Parse(kvp.Key),
                    SelectedOptionId = Guid.Parse(kvp.Value)
                })
                .ToList();

            var submitRequest = new SubmitChallengeRequest { Answers = submittedAnswers };
            var response = await client.PostAsJsonAsync($"/api/challenges/{id}/submit", submitRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                TempData["ErrorMessage"] = error?.Message ?? "Failed to submit challenge.";
                return RedirectToAction(nameof(Take), new { id });
            }

            return RedirectToAction(nameof(Result), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting challenge {AttemptId}.", id);
            TempData["ErrorMessage"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(Take), new { id });
        }
    }

    /// <summary>
    /// Displays the score result page after challenge completion.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Result(Guid id)
    {
        ViewData["Title"] = "Challenge Result";
        try
        {
            var client = GetApiClient();
            AttachBearerToken(client);

            var response = await client.GetAsync($"/api/challenges/{id}/result");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load the challenge result.";
                return RedirectToAction(nameof(Index));
            }

            var result = await response.Content.ReadFromJsonAsync<ChallengeResultDto>();
            if (result == null)
            {
                TempData["ErrorMessage"] = "Could not load the challenge result.";
                return RedirectToAction(nameof(Index));
            }

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading result for challenge {AttemptId}.", id);
            TempData["ErrorMessage"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Displays the full answer review for a completed challenge.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Review(Guid id)
    {
        ViewData["Title"] = "Challenge Review";
        try
        {
            var client = GetApiClient();
            AttachBearerToken(client);

            var response = await client.GetAsync($"/api/challenges/{id}/review");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load the challenge review.";
                return RedirectToAction(nameof(Result), new { id });
            }

            var review = await response.Content.ReadFromJsonAsync<ChallengeReviewDto>();
            if (review == null)
            {
                TempData["ErrorMessage"] = "Could not load the challenge review.";
                return RedirectToAction(nameof(Result), new { id });
            }

            return View(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading review for challenge {AttemptId}.", id);
            TempData["ErrorMessage"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(Result), new { id });
        }
    }

    // ─── Private Helpers ───────────────────────────────────────────────────────

    private HttpClient GetApiClient()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var request = HttpContext.Request;
        var hostPort = request.Host.Port;

        if (hostPort == 44373 || hostPort == 64202)
        {
            client.BaseAddress = new Uri(request.IsHttps ? "https://localhost:44305" : "http://localhost:64153");
        }
        else if (hostPort == 7246 || hostPort == 5251)
        {
            client.BaseAddress = new Uri(request.IsHttps ? "https://localhost:7172" : "http://localhost:5057");
        }

        return client;
    }

    private void AttachBearerToken(HttpClient client)
    {
        var token = User.FindFirstValue("Token");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
