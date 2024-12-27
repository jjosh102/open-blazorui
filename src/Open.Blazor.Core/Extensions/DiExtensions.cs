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
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(serviceProvider =>
        {
            if (TryGetEnvironmentVariable("OLLAMA_BASE_URL", out var ollamaBaseUrl)) return new Config(ollamaBaseUrl);

            return new Config(Default.BaseUrl);
        });
        services.AddSingleton<IChatClient>(static serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var ollamaConnectionString = config.GetConnectionString("ollama-phi3-5");
            var connectionBuilder = new DbConnectionStringBuilder {
                ConnectionString = ollamaConnectionString
            };
            
            var endpoint = connectionBuilder["Endpoint"].ToString() ?? string.Empty;
            var model = connectionBuilder["Model"].ToString() ?? string.Empty;
            Console.WriteLine($"model: {model}");
            IChatClient chatClient = new OllamaApiClient(new Uri(endpoint),model); 
            return chatClient;
        });
        
        services.AddChatServiceAsScoped();
        services.AddOllamaServiceAsScoped();
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
        
        services.AddOllamaServiceAsScoped();
        services.AddSpeechRecognition();
        return services;
    }

    private static bool TryGetEnvironmentVariable(string variableName, out string value)
    {
        value = Environment.GetEnvironmentVariable(variableName) ?? string.Empty;
        return !string.IsNullOrEmpty(value);
    }
}