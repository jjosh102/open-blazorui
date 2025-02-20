using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;
using Open.Blazor.Core.Models;
using Open.Blazor.Core.Models.Enums;
using Open.Blazor.Core.Services;
using Open.Blazor.Ui.Components.Toast;
using Toolbelt.Blazor.SpeechRecognition;

namespace Open.Blazor.Ui.Components.Chat;

public partial class Chat : ComponentBase, IAsyncDisposable
{
    private readonly ChatService _chatService;
    private readonly OllamaService _ollamaService;
    private readonly IJSRuntime _jsRuntime;
    private readonly SpeechRecognition _speechRecognition;
    private readonly ToastService _toastService;

    private Ollama? _activeOllamaModels;
    private CancellationTokenSource _cancellationTokenSource = new();
    private string? _chatSystemPrompt;
    private Discourse _discourse = new();
    private double _frequencyPenalty;
    private bool _isChatOngoing;
    private bool _isListening;
    private bool _isOllamaUp;
    private HostMode _hostMode = HostMode.Ollama;

    private IJSObjectReference? _jsModule;
    private ElementReference _textAreaReference;

    //todo support history
    private Kernel _kernel = default!;
    private int _maxTokens = 2000;
    private double _presencePenalty;

    //Speech recognition
    private bool _isSpeechAvailable = false;
    private SpeechRecognitionResult[] _results = [];
    private OllamaModel _selectedModel = default!;

    private IList<string> _stopSequences = default!;

    //chat settings
    private double _temperature = 1;
    private double _topP = 1;
    private string _userMessage = string.Empty;

    private bool _isPanelOpen;

    [Parameter]
    [SupplyParameterFromQuery(Name = "selectedHost")]
    public string SelectedHost { get; set; }

    public Chat(ChatService chatService,
        OllamaService ollamaService,
        IJSRuntime jsRuntime,
        SpeechRecognition speechRecognition,
        ToastService toastService)
    {
        _chatService = chatService;
        _ollamaService = ollamaService;
        _jsRuntime = jsRuntime;
        _speechRecognition = speechRecognition;
        _toastService = toastService;
    }

    protected override void OnInitialized()
    {
        if (!string.IsNullOrEmpty(SelectedHost) &&
            Enum.TryParse<HostMode>(SelectedHost, out var parsedMode))
        {
            _hostMode = parsedMode;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _speechRecognition.Lang = "en-US";
        _speechRecognition.InterimResults = false;
        _speechRecognition.Continuous = true;
        _speechRecognition.Result += OnSpeechRecognized;

        if (_hostMode == HostMode.Ollama)
        {
            var result = await _ollamaService.GetListOfLocalModelsAsync();

            if (!result.IsSuccess)
            {
                await ShowError("Ollama service is down. Please ensure Ollama is up and running.");
                return;
            }

            _activeOllamaModels = result.Value;

            if (_activeOllamaModels is null || !_activeOllamaModels.Models.Any())
            {
                await ShowError("No models found");
                return;
            }

            var defaultModel = _activeOllamaModels.Models.First();
            _kernel = _chatService.CreateKernel(defaultModel.Name);
            _selectedModel = defaultModel;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Chat/Chat.razor.js");
            await _jsModule.InvokeVoidAsync("initializeChatHandlers");
            await _jsModule.InvokeVoidAsync("adjustChatWindowHeightAndScroll");
            await ScrollToBottom();
            _isSpeechAvailable = await _speechRecognition.IsAvailableAsync();
            StateHasChanged();
        }
    }

    private async Task SendMessage()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_userMessage)) return;
    
            _isChatOngoing = true;
            var modelName = _hostMode == HostMode.Aspire
                ? _chatService.GetCurrentModel
                : _selectedModel.Name;
            _discourse.AddChatMessage(MessageRole.User, _userMessage, modelName);
            _discourse.AddChatMessage(MessageRole.Assistant, string.Empty, modelName);
            _userMessage = string.Empty;
            await StopListening();

            if (_hostMode == HostMode.Aspire)
            {
                await _chatService.StreamChatMessageContentAsync(
                    _discourse, OnStreamCompletion, _cancellationTokenSource.Token);
            }
            else
            {
                var settings = ChatSettings.New(
                    _temperature, _topP, _presencePenalty, _frequencyPenalty,
                    _maxTokens, default, _chatSystemPrompt);

                await _chatService.StreamChatMessageContentAsync(
                    _kernel, _discourse, OnStreamCompletion, settings,
                    _cancellationTokenSource.Token);
            }


            _discourse.ChatMessages.Last().IsDoneStreaming = true;
            
            //reset textbox height
            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("adjustChatWindowHeightAndScroll");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessage: {ex.Message}");
        }
        finally
        {
            ResetCancellationTokenSource();
            _isChatOngoing = false;
        }
    }

    private async Task OnStreamCompletion(string updatedContent)
    {
        _discourse.ChatMessages.Last().Content += updatedContent;
        await ScrollToBottom();
    }

    private void ResetCancellationTokenSource()
    {
        if (_cancellationTokenSource.TryReset()) return;
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async Task ShowError(string errorMessage)
    {
        await _toastService.ShowToastAsync(
            message: errorMessage,
            type: ToastType.Error,
            title: "Error",
            durationMs: 3000
        );
    }

    private void HandleSelectedOptionChanged(ChangeEventArgs e)
    {
        var selectedName = e.Value?.ToString();
        if (string.IsNullOrEmpty(selectedName)) return;

        var selectedModel = _activeOllamaModels?.Models?.FirstOrDefault(m => m.Name == selectedName);
        if (selectedModel != null)
        {
            _selectedModel = selectedModel;
        }

        _kernel = _chatService.CreateKernel(_selectedModel.Name);
    }

    private async Task StopChat() => await _cancellationTokenSource.CancelAsync();

    private async Task ScrollToBottom()
    {
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("scrollToBottom");
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnSpeechRecognized(object? sender, SpeechRecognitionEventArgs args)
    {
        if (args.Results == null || args.Results.Length <= args.ResultIndex) return;

        var transcript = new StringBuilder(_userMessage);
        foreach (var result in args.Results.Skip(args.ResultIndex))
            if (result.IsFinal)
                transcript.Append(result.Items![0].Transcript);

        _userMessage = transcript.ToString();

        StateHasChanged();
    }
    
    private async ValueTask<bool> EnsureDeviceIsAvailable()
    {
        if (_isSpeechAvailable) return true;
        await ShowError("Device not available");
        return false;
    }

    private async Task StartListening()
    {
        var isDeviceAvailable = await EnsureDeviceIsAvailable();
        if (!isDeviceAvailable)
            return;

        if (!_isListening)
        {
            _isListening = true;
            await _speechRecognition.StartAsync();
            await _toastService.ShowToastAsync(
                message: "Listening...",
                type: ToastType.Success,
                title: "",
                durationMs: 3000
            );
        }
    }

    private async Task StopListening()
    {
        var isDeviceAvailable = await EnsureDeviceIsAvailable();
        if (!isDeviceAvailable)
            return;

        if (_isListening)
        {
            _isListening = false;
            await _speechRecognition.StopAsync();
            await _toastService.ShowToastAsync(
                message: "Stopped listening...",
                type: ToastType.Info,
                title: "",
                durationMs: 3000
            );
        }
    }

    private void TogglePanel() => _isPanelOpen = !_isPanelOpen;

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource?.Dispose();
        _speechRecognition.Result -= OnSpeechRecognized;
        try
        {
            if (_jsModule is not null)
                await _jsModule.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }
}