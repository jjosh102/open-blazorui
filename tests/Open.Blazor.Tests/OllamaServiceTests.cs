using System.Net;
using System.Text.Json;
using FluentAssertions;
using Open.Blazor.Core.Constants;
using Open.Blazor.Core.Models;
using Open.Blazor.Core.Services;

namespace Open.Blazor.Tests.OllamaServiceTests;

public class MockHttpMessageHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync ?? throw new ArgumentNullException(nameof(sendAsync));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return _sendAsync(request, cancellationToken);
    }
}

public class OllamaServiceTests
{
    private readonly HttpClient _client;
    private readonly Config _config;
    private HttpResponseMessage _response;

    public OllamaServiceTests()
    {
        var handler = new MockHttpMessageHandler((request, cancellationToken) => Task.FromResult(_response));
        _client = new HttpClient(handler);
        _config = new Config(Default.BaseUrl);
    }

    [Fact]
    public async Task GetListOfLocalModels_ShouldReturnOllama_WhenResponseIsSuccessful()
    {
        // Arrange
        var ollama = new Ollama();
        var jsonResponse = JsonSerializer.Serialize(ollama);
        _response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse)
        };

        var service = new OllamaService(_client, _config);

        // Act
        var result = await service.GetListOfLocalModelsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(ollama);
    }

    [Fact]
    public async Task GetListOfLocalModels_ShouldReturnFailure_WhenResponseBodyIsNull()
    {
        // Arrange
        _response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(string.Empty) // Empty response body
        };

        var service = new OllamaService(_client, _config);

        // Act
        var result = await service.GetListOfLocalModelsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.ErrorType.Should().Be(Error.NullValue.ErrorType);
    }

    [Fact]
    public async Task GetListOfLocalModels_WithCustomBaseUrl_ShouldReturnOllama_WhenResponseIsSuccessful()
    {
        // Arrange
        var ollama = new Ollama();
        var jsonResponse = JsonSerializer.Serialize(ollama);
        _response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse)
        };

        var service = new OllamaService(_client, _config);
        var customBaseUrl = Default.BaseUrl;

        // Act
        var result = await service.GetListOfLocalModelsAsync(customBaseUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(ollama);
    }

    [Fact]
    public async Task GetListOfLocalModels_ShouldThrowArgumentNullException_WhenBaseUrlIsNull()
    {
        // Arrange
        var service = new OllamaService(_client, _config);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetListOfLocalModelsAsync(null));
    }

    [Fact]
    public async Task GetListOfLocalModels_ShouldThrowArgumentException_WhenBaseUrlIsEmpty()
    {
        // Arrange
        var service = new OllamaService(_client, _config);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetListOfLocalModelsAsync(string.Empty));
    }
}