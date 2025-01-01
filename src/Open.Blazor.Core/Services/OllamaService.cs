using System.Text.Json;
using Open.Blazor.Core.Models;

namespace Open.Blazor.Core.Services;

public sealed class OllamaService
{
    private readonly Config _config;
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient, Config config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<Result<Ollama>> GetListOfLocalModelsAsync()
    {
        return await GetLocalModels(_config.OllamaUrl);
    }

    public async Task<Result<Ollama>> GetListOfLocalModelsAsync(string baseUrl)
    {
        return await GetLocalModels(baseUrl);
    }


    private async Task<Result<Ollama>> GetLocalModels(string baseUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);
        try
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
            var response = await _httpClient.GetAsync("api/tags");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStreamAsync();
            if (responseBody is null) return Result.Failure<Ollama>(Error.NullValue);

            return JsonSerializer.Deserialize<Ollama>(responseBody)!;
        }
        catch (Exception ex)
        {
            return Result.Failure<Ollama>(Error.Failure(ex.Message));
        }
    }
}

