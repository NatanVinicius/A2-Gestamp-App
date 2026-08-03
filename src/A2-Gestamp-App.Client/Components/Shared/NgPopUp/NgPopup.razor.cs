using A2GestampApp.Application.Features.Ng;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Shared.NgPopUp;

public partial class NgPopup : ComponentBase, IDisposable
{
  [Inject]
  private INgState NgState { get; set; } = default!;

  [Inject]
  private NavigationManager Navigation { get; set; } = default!;

  private bool _navigationScheduled;

  protected override void OnInitialized()
  {
    NgState.StateChanged += OnStateChanged;
  }

  private async Task OnStartRecognition()
  {
    await NgState.StartRecognitionAsync();
  }

  private void OnStateChanged()
  {
    if (NgState.Status == FaceRecognitionStatus.Success &&
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
      NgState.Close();

      if (!Navigation.Uri.EndsWith("/control"))
      {
        Navigation.NavigateTo("/control");
      }

      _navigationScheduled = false;
    });
  }

  public void Dispose()
  {
    NgState.StateChanged -= OnStateChanged;
  }
}
