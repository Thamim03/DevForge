using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using DevForge.Web.Models;
using DevForge.Application.Common.Models;

namespace DevForge.Web.Controllers;

/// <summary>
/// Controller handling MVC-based user account interactions (Login, Register, Logout).
/// </summary>
public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = GetApiClient();
            var apiResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                UsernameOrEmail = model.UsernameOrEmail,
                Password = model.Password
            });

            if (!apiResponse.IsSuccessStatusCode)
            {
                var errorObj = await apiResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
                ModelState.AddModelError(string.Empty, errorObj?.Message ?? "Invalid username/email or password.");
                return View(model);
            }

            var authResponse = await apiResponse.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
            {
                ModelState.AddModelError(string.Empty, "Authentication failed. Token not received.");
                return View(model);
            }

            // Create user claims from API response
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, authResponse.Id.ToString()),
                new(ClaimTypes.Name, authResponse.Username),
                new(ClaimTypes.Email, authResponse.Email),
                new(ClaimTypes.Role, authResponse.Role),
                new("Token", authResponse.Token) // Store the token in the claims for easier API authentication
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Store token in the authentication session properties
            authProperties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = authResponse.Token }
            });

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User {Username} logged in successfully.", authResponse.Username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login post.");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = GetApiClient();
            var apiResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            });

            if (!apiResponse.IsSuccessStatusCode)
            {
                var errorObj = await apiResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
                ModelState.AddModelError(string.Empty, errorObj?.Message ?? "Registration failed. Username or Email may be taken.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Registration successful! Please login with your credentials.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration post.");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

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

    private class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
