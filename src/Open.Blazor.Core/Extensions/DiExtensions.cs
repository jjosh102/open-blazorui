using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using Open.Blazor.Core.Constants;
using Open.Blazor.Core.Models;
using Open.Blazor.Core.Services;

namespace Open.Blazor.Core.Extensions;

public static class DiExtensions
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(serviceProvider =>
        {
            if (TryGetEnvironmentVariable("OLLAMA_BASE_URL", out var ollamaBaseUrl)) return new Config(ollamaBaseUrl);

            return new Config(Default.BaseUrl);
        });

        // Configure based on the model identified in AppHost
        if (TryGetDbConfig(configuration, "ollama-phi3-5", out var endpoint, out var model))
        {
            services.AddSingleton<IChatClient>(_ => new OllamaApiClient(new Uri(endpoint), model));
        }else
        {
            services.AddSingleton<IChatClient>(_ => new OllamaApiClient(Default.BaseUrl, Default.Model));
        }

        services.AddChatServiceAsScoped();
        services.AddHttpClient<OllamaService>();
        services.AddSpeechRecognition();
        return services;
    }

    public static IServiceCollection AddWsCoreDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(serviceProvider =>
        {
            if (TryGetEnvironmentVariable("OLLAMA_BASE_URL", out var ollamaBaseUrl)) return new Config(ollamaBaseUrl);

            return new Config(Default.BaseUrl);
        });

        services.AddHttpClient<OllamaService>();
        services.AddSpeechRecognition();
        return services;
    }

    private static bool TryGetEnvironmentVariable(string variableName, out string value)
    {
        value = Environment.GetEnvironmentVariable(variableName) ?? string.Empty;
        return !string.IsNullOrEmpty(value);
    }
    private static bool TryGetDbConfig(IConfiguration configuration, string connectionName, out string endpoint, out string model)
    {
        endpoint = string.Empty;
        model = string.Empty;

        var connectionString = configuration.GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        var connectionBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        endpoint = connectionBuilder["Endpoint"]?.ToString() ?? string.Empty;
        model = connectionBuilder["Model"]?.ToString() ?? string.Empty;


        return !string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(model);
    }
}