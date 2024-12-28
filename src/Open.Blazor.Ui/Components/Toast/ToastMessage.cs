namespace Open.Blazor.Ui.Components.Toast;

public class ToastMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public ToastType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int DurationMs { get; set; }
    public bool IsVisible { get; set; } = true;
}