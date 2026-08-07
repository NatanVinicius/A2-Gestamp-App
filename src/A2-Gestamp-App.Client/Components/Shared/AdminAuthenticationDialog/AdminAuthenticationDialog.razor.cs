using A2GestampApp.Application.Features.AdminAuthentication;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Shared.AdminAuthenticationDialog;

public partial class AdminAuthenticationDialog
    : ComponentBase,
      IDisposable
{
  [Inject]
  private IAdminAuthenticationState State { get; set; } = default!;

  [Inject]
  private NavigationManager Navigation { get; set; } = default!;

  private bool _navigationScheduled;

  protected override void OnInitialized()
  {
    State.StateChanged += OnStateChanged;
  }

  private async Task OnStartRecognition()
  {
    await State.StartRecognitionAsync();
  }

  private void OnStateChanged()
  {
    if (State.Status == FaceRecognitionStatus.Success &&
        !_navigationScheduled)
    {
      _navigationScheduled = true;

      _ = NavigateAfterSuccessAsync();
    }

    _ = InvokeAsync(StateHasChanged);
  }

  private async Task NavigateAfterSuccessAsync()
  {
    await Task.Delay(1000);

    await InvokeAsync(() =>
    {
      State.Close();

      Navigation.NavigateTo("/signup");

      _navigationScheduled = false;
    });
  }

  private void OnClose()
  {
    State.Close();

    Navigation.NavigateTo("/");
  }

  public void Dispose()
  {
    State.StateChanged -= OnStateChanged;
  }
}
