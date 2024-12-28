using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Open.Blazor.Core.Models;

namespace Open.Blazor.Core.Services;

public sealed class ChatService
{
    private readonly Config _config;
    private readonly IChatClient _client;

    public ChatService(Config config, IChatClient client)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Kernel CreateKernel(string model)
    {
        var kernelBuilder = Kernel.CreateBuilder()
            .AddOllamaChatCompletion(
                model,
                new Uri(_config.OllamaUrl));

        return kernelBuilder.Build();
    }


    public async Task StreamChatMessageContentAsync(Kernel kernel,
        Discourse discourse,
        Func<string, Task> onStreamCompletion,
        ChatSettings chatSettings,
        CancellationToken cancellationToken = default)
    {

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        var executionSettings = chatSettings.ToOpenAIPromptExecutionSettings();
        var history = discourse.ToChatHistory();
        var chatCompletionStream =
            chatCompletion.GetStreamingChatMessageContentsAsync(history, executionSettings,
                cancellationToken: cancellationToken);
        await foreach (var completionResult in chatCompletionStream)
        {
            if (cancellationToken.IsCancellationRequested) return;

            await onStreamCompletion.Invoke(completionResult.Content ?? string.Empty);
        }
    }

    public async Task StreamChatMessageContentAsync(Discourse discourse,
        Func<string, Task> onStreamCompletion,
        CancellationToken cancellationToken = default)
    {
        var history = discourse.ToChatMessages();
        await foreach (var completionResult in _client.CompleteStreamingAsync(history, null, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested) return;

            await onStreamCompletion.Invoke(completionResult.Text ?? string.Empty);
        }
    }
    public string GetCurrentModel => _client.Metadata.ModelId ?? "No Model Found";
}

public static class ChatServiceExtensions
{
    public static IServiceCollection AddChatServiceAsScoped(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ChatService>();
        return services;
    }
}