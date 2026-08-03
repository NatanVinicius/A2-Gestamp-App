using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Shared.ConfirmationDialog;

public partial class ConfirmationDialog : ComponentBase, IDisposable
{
  [Inject]
  private IConfirmationDialogState ConfirmationState { get; set; } = default!;

  protected override void OnInitialized()
  {
    ConfirmationState.StateChanged += OnStateChanged;
  }

  private async Task OnConfirm()
  {
    await ConfirmationState.ConfirmAsync();
  }

  private void OnStateChanged()
  {
    InvokeAsync(StateHasChanged);
  }

  public void Dispose()
  {
    ConfirmationState.StateChanged -= OnStateChanged;
  }
}
