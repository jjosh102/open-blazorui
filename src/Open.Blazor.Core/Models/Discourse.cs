using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Open.Blazor.Core.Models.Enums;

namespace Open.Blazor.Core.Models;

public class Discourse
{
    public Guid? Id { get; set; }
    public string Model { get; set; } = default!;
    public List<MessageContent> ChatMessages { get; set; } = [];

    public void AddChatMessage(MessageRole role, string content, string model)
    {
        ChatMessages.Add(MessageContent.New(role, content, model));
    }
}

public static class DiscourseExtensions
{
    private static AuthorRole MapToAuthorRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.System => AuthorRole.System,
            MessageRole.User => AuthorRole.User,
            _ => AuthorRole.Assistant
        };
    }

    private static ChatRole MapToChatRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.System => ChatRole.System,
            MessageRole.User => ChatRole.User,
            _ => ChatRole.Assistant
        };
    }

    private static MessageRole MapToMessageRole(AuthorRole role)
    {
        if (role == AuthorRole.System)
            return MessageRole.System;

        if (role == AuthorRole.User)
            return MessageRole.User;

        return MessageRole.Assistant;
    }

    private static MessageRole MapToMessageRole(ChatRole role)
    {
        if (role == ChatRole.System)
            return MessageRole.System;

        if (role == ChatRole.User)
            return MessageRole.User;

        return MessageRole.Assistant;
    }


    public static ChatHistory ToChatHistory(this Discourse discourse)
    {
        var chatHistory = new ChatHistory();
        foreach (var chat in discourse.ChatMessages.Where(c => !string.IsNullOrWhiteSpace(c.Content)))
        {
            var role = MapToAuthorRole(chat.Role);
            chatHistory.AddMessage(role, chat.Content);
        }

        return chatHistory;
    }

    public static List<ChatMessage> ToChatMessages(this Discourse discourse)
    {
        return discourse.ChatMessages
            .Where(c => !string.IsNullOrWhiteSpace(c.Content))
            .Select(chat => new ChatMessage(MapToChatRole(chat.Role), chat.Content))
            .ToList();
    }

    public static Discourse ToDiscourse(this ChatHistory chatHistory, string model)
    {
        var discourse = new Discourse();
        foreach (var chat in chatHistory)
        {
            var role = MapToMessageRole(chat.Role);
            discourse.AddChatMessage(role, chat.Content ?? string.Empty, model);
        }

        return discourse;
    }

    public static Discourse ToDiscourse(this IEnumerable<ChatMessage> chatHistory, string model)
    {
        var discourse = new Discourse();
        foreach (var chat in chatHistory)
        {
            var role = MapToMessageRole(chat.Role);
            discourse.AddChatMessage(role, chat.Text ?? string.Empty, model);
        }

        return discourse;
    }
}