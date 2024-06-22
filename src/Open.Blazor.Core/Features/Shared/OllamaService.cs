﻿
using Microsoft.Extensions.DependencyInjection;
using Open.Blazor.Core.Features.Shared.Models;
using System.Text.Json;


namespace Open.Blazor.Core.Features.Shared
{
  public sealed class OllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly Config _config;

        public OllamaService(HttpClient httpClient, Config config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<Result<Ollama>> GetListOfLocalModelsAsync() =>
            await GetLocalModels(_config.OllamaUrl);

        public async Task<Result<Ollama>> GetListOfLocalModelsAsync(string baseUrl) =>
             await GetLocalModels(baseUrl);


        private async Task<Result<Ollama>> GetLocalModels(string baseUrl)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(baseUrl);
            try
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
                var response = await _httpClient.GetAsync("api/tags");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStreamAsync();
                if (responseBody is null)
                {
                    return Result.Failure<Ollama>(Error.NullValue);
                }

                return JsonSerializer.Deserialize<Ollama>(responseBody)!;
            }
            catch (Exception ex)
            {
                return Result.Failure<Ollama>(Error.Failure(ex.Message));
            }
        }

    }

    public static class OllamaServiceExensions
    {
        public static IServiceCollection AddOllamaServiceAsScoped(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddHttpClient<OllamaService>();
            return services;
        }
    }
}
