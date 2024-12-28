using Open.Blazor.Core.Constants;
using Open.Blazor.Core.Models;
using Open.Blazor.Core.Services;

namespace Open.Blazor.Tests.ChatServiceTests;

public class ChatServiceTests
{
    private const string Model = "llama3:latest";
    private readonly Config _config;

    public ChatServiceTests()
    {
        _config = new Config(Default.BaseUrl); // Provide a default URL or mock config
    }

    [Fact]
    public void Constructor_ShouldInitialize_WhenParametersAreValid()
    {
        var service = new ChatService(_config,null);
        Assert.NotNull(service);
    }

    [Fact]
    public void CreateKernel_ShouldThrowArgumentNullException_WhenModelIsNull()
    {
        var service = new ChatService(_config,null);
        Assert.Throws<ArgumentNullException>(() => service.CreateKernel(null));
    }

    [Fact]
    public void CreateKernel_ShouldReturnKernel_WhenModelIsValid()
    {
        var service = new ChatService(_config,null);
        var kernel = service.CreateKernel(Model);
        Assert.NotNull(kernel);
    }

    [Fact]
    public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldThrowArgumentNullException_WhenKernelIsNull()
    {
        var service = new ChatService(_config,null);
        var discourse = new Discourse();
        var chatSettings = new ChatSettings();
        Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.StreamChatMessageContentAsync(null, discourse, onStreamCompletion, chatSettings));
    }

    [Fact]
    public async Task
        StreamChatMessageContentAsync_WithChatSettings_ShouldThrowArgumentNullException_WhenDiscourseIsNull()
    {
        var service = new ChatService(_config,null);
        var kernel = service.CreateKernel(Model);
        var chatSettings = new ChatSettings();
        Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.StreamChatMessageContentAsync(kernel, null, onStreamCompletion, chatSettings));
    }

    [Fact]
    public async Task
        StreamChatMessageContentAsync_WithChatSettings_ShouldThrowArgumentNullException_WhenOnStreamCompletionIsNull()
    {
        var service = new ChatService(_config,null);
        var kernel = service.CreateKernel(Model);
        var discourse = new Discourse();
        var chatSettings = new ChatSettings();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.StreamChatMessageContentAsync(kernel, discourse, null, chatSettings));
    }

    [Fact]
    public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldReturnDiscourse_WhenCancellationIsRequested()
    {
        var service = new ChatService(_config,null);
        var kernel = service.CreateKernel(Model);
        var discourse = new Discourse();
        var chatSettings = new ChatSettings();
        Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;

        using (var cts = new CancellationTokenSource())
        {
            var task = service.StreamChatMessageContentAsync(kernel, discourse, onStreamCompletion, chatSettings,
                cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(() => task);
        }
    }

    [Fact]
    public async Task StreamChatMessageContentAsync_ShouldThrowArgumentNullException_WhenDiscourseIsNull()
    {
        var service = new ChatService(_config,null);
        var chatSettings = new ChatSettings();
        Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.StreamChatMessageContentAsync(null, onStreamCompletion));
    }

    [Fact]
    public async Task StreamChatMessageContentAsync_ShouldThrowArgumentNullException_WhenOnStreamCompletionIsNull()
    {
        var service = new ChatService(_config,null);
        var discourse = new Discourse();
        var chatSettings = new ChatSettings();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.StreamChatMessageContentAsync(discourse, null));
    }
    
}