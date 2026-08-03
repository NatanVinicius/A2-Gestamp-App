public interface IConfirmationDialogState
{
  public bool IsOpen { get; }

  public string Title { get; }

  public string Message { get; }

  public string ConfirmText { get; }

  public event Action? StateChanged;

  public void Open(
      string title,
      string message,
      string confirmText,
      Func<Task> onConfirm);

  public Task ConfirmAsync();

  public void Close();
}
