using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using DevForge.Application.Common.Models;
using DevForge.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DevForge.IntegrationTests;

public class ToolsIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _sqliteConnection;
    private readonly MockHttpMessageHandler _mockHandler;

    public ToolsIntegrationTests()
    {
        _sqliteConnection = new SqliteConnection("Data Source=DevForgeToolsTestDb;Mode=Memory;Cache=Shared");
        _sqliteConnection.Open();

        _mockHandler = new MockHttpMessageHandler();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Data Source=DevForgeToolsTestDb;Mode=Memory;Cache=Shared");
            
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient("ApiPlaygroundClient")
                    .ConfigurePrimaryHttpMessageHandler(() => _mockHandler);
            });
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task FormatSql_Should_ReturnFormattedSqlSuccessfully()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SqlFormatterRequest { Sql = "select * from users where id = 1" };

        // Act
        var response = await client.PostAsJsonAsync("/api/tools/sql/format", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SqlFormatterResponse>();
        result.Should().NotBeNull();
        result!.IsValid.Should().BeTrue();
        result.FormattedSql.Should().Contain("SELECT");
        result.FormattedSql.Should().Contain("FROM");
        result.FormattedSql.Should().Contain("WHERE");
    }

    [Fact]
    public async Task MinifySql_Should_ReturnMinifiedSqlSuccessfully()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SqlFormatterRequest { Sql = "SELECT * \n FROM users \n WHERE id = 1" };

        // Act
        var response = await client.PostAsJsonAsync("/api/tools/sql/minify", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SqlFormatterResponse>();
        result.Should().NotBeNull();
        result!.IsValid.Should().BeTrue();
        result.FormattedSql.Should().Be("SELECT * FROM users WHERE id=1");
    }

    [Fact]
    public async Task ExecuteHttpRequest_WithoutToken_Should_ReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ApiPlaygroundRequest
        {
            Method = "GET",
            Url = "https://api.github.com/users/octocat"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/tools/http/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExecuteHttpRequest_WithToken_Should_ExecuteAndReturnResponse()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Log in to get token
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            UsernameOrEmail = "user@devforge.com", // Seeded
            Password = "User123!"
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Trigger request to an external API (mocked internally or public API)
        var request = new ApiPlaygroundRequest
        {
            Method = "GET",
            Url = "https://api.github.com/users/octocat"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/tools/http/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiPlaygroundResponse>();
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Body.Should().Contain("mocked response");
    }

    [Fact]
    public async Task ExecuteHttpRequest_WithLocalhost_Should_BeBlockedBySSRF()
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

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var request = new ApiPlaygroundRequest
        {
            Method = "GET",
            Url = "http://localhost:5057/api/system/status"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/tools/http/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiPlaygroundResponse>();
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400); // Blocked
        result.ErrorMessage.Should().Contain("localhost");
    }

    public void Dispose()
    {
        _factory.Dispose();
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
    }

    private class MockHttpMessageHandler : System.Net.Http.HttpMessageHandler
    {
        protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(new System.Net.Http.HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new System.Net.Http.StringContent("{\"status\":\"success\",\"message\":\"mocked response\"}", System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
