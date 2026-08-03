public sealed class ConfirmationDialogState
    : IConfirmationDialogState
{
  private Func<Task>? _onConfirm;

  public bool IsOpen { get; private set; }

  public string Title { get; private set; } = string.Empty;

  public string Message { get; private set; } = string.Empty;

  public string ConfirmText { get; private set; } = "Confirmar";

  public event Action? StateChanged;

  public void Open(
      string title,
      string message,
      string confirmText,
      Func<Task> onConfirm)
  {
    Title = title;
    Message = message;
    ConfirmText = confirmText;

    _onConfirm = onConfirm;

    IsOpen = true;

    StateChanged?.Invoke();
  }

  public async Task ConfirmAsync()
  {
    if (_onConfirm is not null)
    {
      await _onConfirm();
    }

    Close();
  }

  public void Close()
  {
    IsOpen = false;

    StateChanged?.Invoke();
  }
}
