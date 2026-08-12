using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using DevForge.Application.Common.Models;
using DevForge.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DevForge.IntegrationTests;

public class AuthEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _sqliteConnection;

    public AuthEndpointsTests()
    {
        // Keep the database alive
        _sqliteConnection = new SqliteConnection("Data Source=DevForgeAuthTestDb;Mode=Memory;Cache=Shared");
        _sqliteConnection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Data Source=DevForgeAuthTestDb;Mode=Memory;Cache=Shared");
        });

        // Ensure database schema is ready and seeded
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Register_WithValidData_Should_ReturnCreatedAndToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Username = "new_integration_user",
            Email = "new_integration@devforge.com",
            Password = "Integration123!",
            ConfirmPassword = "Integration123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Username.Should().Be(request.Username);
        content.Email.Should().Be(request.Email);
        content.Token.Should().NotBeNullOrEmpty();
        content.Role.Should().Be("User");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Should_ReturnConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest
        {
            Username = "duplicate_user",
            Email = "user@devforge.com", // Seeded email
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Should_ReturnOkAndToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginRequest
        {
            UsernameOrEmail = "user@devforge.com", // Seeded
            Password = "User123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Email.Should().Be(request.UsernameOrEmail);
        content.Token.Should().NotBeNullOrEmpty();
        content.Role.Should().Be("User");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Should_ReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginRequest
        {
            UsernameOrEmail = "user@devforge.com",
            Password = "WrongPassword!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WhenUnauthenticated_Should_ReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WhenAuthenticated_Should_ReturnUserDetails()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Log in to get token
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            UsernameOrEmail = "user@devforge.com",
            Password = "User123!"
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();

        // Attach token
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Email.Should().Be("user@devforge.com");
        user.Role.Should().Be("User");
    }

    [Fact]
    public async Task AdminCheck_AsNormalUser_Should_ReturnForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Log in as normal user
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            UsernameOrEmail = "user@devforge.com",
            Password = "User123!"
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act
        var response = await client.GetAsync("/api/auth/admin-check");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCheck_AsAdmin_Should_ReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Log in as admin
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            UsernameOrEmail = "admin@devforge.com", // Seeded admin
            Password = "Admin123!"
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act
        var response = await client.GetAsync("/api/auth/admin-check");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();

        // Clean environment variable
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
    }
}
