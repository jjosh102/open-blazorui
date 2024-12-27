using Open.Blazor.Core.Models.Enums;

namespace Open.Blazor.Core.Models;

public class MessageContent
{
    public MessageContent(Guid id, MessageRole role, string content, string model)
    {
        Role = role;
        Content = content;
        Id = id;
        Model = model;
    }

    public Guid Id { get; }
    public MessageRole Role { get; }
    public DateTime Date { get; } = DateTime.Now;
    public string Content { get; set; } = string.Empty;
    public bool IsDoneStreaming { get; set; } = false;
    public string Model { get; }

    public static MessageContent New(MessageRole role, string content, string model)
    {
        return new MessageContent(Guid.NewGuid(), role, content, model);
    }
}