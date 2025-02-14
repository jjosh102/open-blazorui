namespace Open.Blazor.Ui.Components.Toast;

public class ToastService : IDisposable
{
    private readonly List<ToastMessage> _toasts = [];
    private const int DefaultDurationMs = 5000;
    private readonly PeriodicTimer? _periodicTimer;
    private readonly CancellationTokenSource _cts = new();

    public event Action? OnToastsChanged;

    public ToastService()
    {
        _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        _ = CheckToastsPeriodicallyAsync(_cts.Token);
    }

    public async Task ShowToastAsync(string message, ToastType type = ToastType.Info, string title = "", int durationMs = DefaultDurationMs)
    {
        var toast = new ToastMessage
        {
            Message = message,
            Title = title,
            Type = type,
            DurationMs = durationMs,
            IsVisible = false,
            CreatedAt = DateTime.Now
        };

        _toasts.Add(toast);
        OnToastsChanged?.Invoke();

        await Task.Delay(50);
        toast.IsVisible = true;
        OnToastsChanged?.Invoke();
    }

    private async Task CheckToastsPeriodicallyAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Remove expired toasts
            while (await _periodicTimer!.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
            {
                var expiredToasts = _toasts
                    .Where(t => (DateTime.Now - t.CreatedAt).TotalMilliseconds >= t.DurationMs)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();

                foreach (var toast in expiredToasts)
                {
                    await RemoveToast(toast);
                }
            }
        }
        catch (OperationCanceledException)
        {

        }

    }

    public async Task RemoveToast(ToastMessage toast)
    {
        toast.IsVisible = false;
        OnToastsChanged?.Invoke();

        await Task.Delay(500);

        _toasts.Remove(toast);
        OnToastsChanged?.Invoke();
    }

    public List<ToastMessage> GetToasts()
    {
        return _toasts;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _periodicTimer?.Dispose();
        _toasts.Clear();
    }
}