using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using FluentAssertions;
using DevForge.Application.Common.Models;
using DevForge.Application.Services;
using Microsoft.Extensions.Logging;

namespace DevForge.UnitTests.Services;

public class ApiPlaygroundServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<ApiPlaygroundService>> _loggerMock;
    private readonly ApiPlaygroundService _service;

    public ApiPlaygroundServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<ApiPlaygroundService>>();
        _service = new ApiPlaygroundService(_httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("http://localhost")]
    [InlineData("http://localhost:5057")]
    [InlineData("https://127.0.0.1")]
    [InlineData("http://[::1]")]
    [InlineData("http://192.168.1.1")]
    [InlineData("https://10.0.0.5")]
    [InlineData("http://172.16.2.3")]
    [InlineData("http://169.254.169.254")]
    [InlineData("http://test.local")]
    public async Task SendRequestAsync_WithInternalOrPrivateUrls_Should_RejectWithBadRequest(string localUrl)
    {
        // Arrange
        var request = new ApiPlaygroundRequest
        {
            Method = "GET",
            Url = localUrl
        };

        // Act
        var response = await _service.SendRequestAsync(request);

        // Assert
        response.StatusCode.Should().Be(400);
        response.ErrorMessage.Should().ContainAny("localhost", "private", "internal", "allowed");
        response.Body.Should().BeNull();
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("file:///etc/passwd")]
    [InlineData("invalid-url")]
    public async Task SendRequestAsync_WithInvalidProtocolsOrMalformedUrls_Should_RejectWithBadRequest(string badUrl)
    {
        // Arrange
        var request = new ApiPlaygroundRequest
        {
            Method = "GET",
            Url = badUrl
        };

        // Act
        var response = await _service.SendRequestAsync(request);

        // Assert
        response.StatusCode.Should().Be(400);
        response.ErrorMessage.Should().ContainAny("protocols", "format", "empty");
    }

    [Fact]
    public async Task SendRequestAsync_WithInvalidJsonBody_Should_ReturnBadRequest()
    {
        // Arrange
        var request = new ApiPlaygroundRequest
        {
            Method = "POST",
            Url = "https://api.github.com",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = "{ invalid json: "
        };

        // Act
        var response = await _service.SendRequestAsync(request);

        // Assert
        response.StatusCode.Should().Be(400);
        response.ErrorMessage.Should().Contain("Invalid JSON body");
    }

    [Fact]
    public async Task SendRequestAsync_WithValidHttpParameters_Should_ExecuteRequestAndReturnResponse()
    {
        // Arrange
        var request = new ApiPlaygroundRequest
        {
            Method = "POST",
            Url = "https://api.github.com",
            Headers = new Dictionary<string, string> 
            { 
                { "Content-Type", "application/json" },
                { "X-Custom-Header", "Hello" } 
            },
            Body = "{\"name\":\"test\"}"
        };

        // Setup HttpClient Mock
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent("{\"id\":1}", System.Text.Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(_ => _.CreateClient("ApiPlaygroundClient")).Returns(client);

        // Act
        var response = await _service.SendRequestAsync(request);

        // Assert
        response.StatusCode.Should().Be(201);
        response.StatusDescription.Should().Be("Created");
        response.Body.Should().Be("{\"id\":1}");
        response.Headers.Should().ContainKey("Content-Type");
        response.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }
}
