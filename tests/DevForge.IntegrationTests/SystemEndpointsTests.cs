using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using DevForge.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace DevForge.IntegrationTests;

/// <summary>
/// Integration tests verifying system-level endpoints and health checks.
/// </summary>
public class SystemEndpointsTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _sqliteConnection;

    public SystemEndpointsTests()
    {
        // Open a master connection to keep the shared SQLite in-memory database alive
        _sqliteConnection = new SqliteConnection("Data Source=DevForgeTestDb;Mode=Memory;Cache=Shared");
        _sqliteConnection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Data Source=DevForgeTestDb;Mode=Memory;Cache=Shared");
        });

        // Ensure the database schema is created on the SQLite instance
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetStatus_Should_ReturnOkAndValidJsonPayload()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/system/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<SystemStatusResponse>();
        content.Should().NotBeNull();
        content!.Status.Should().Be("Healthy");
        content.Application.Should().Be("DevForge");
    }

    [Fact]
    public async Task HealthEndpoint_Should_ReturnHealthyAndOkStatus()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    public void Dispose()
    {
        _factory.Dispose();
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();
        
        // Clean up environment variable
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
    }

    private class SystemStatusResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
    }
}
