using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Shared.FaceCaptureDialog;

public partial class FaceCaptureDialog
    : ComponentBase,
      IDisposable
{
  [Inject]
  public required IFaceCaptureState FaceCaptureState { get; init; }

  [Inject]
  public required ISignUpState SignUpState { get; init; }

  [Inject]
  public required NavigationManager Navigation { get; init; }

  protected override void OnInitialized()
  {
    FaceCaptureState.StateChanged += OnStateChanged;
  }

  private void OnStateChanged()
  {
    InvokeAsync(StateHasChanged);
  }

  private async Task OnStartCapture()
  {
    await FaceCaptureState.StartCaptureAsync(
        SignUpState.EmployeeId,
        SignUpState.Name,
        SignUpState.Role);

    if (FaceCaptureState.Status == FaceCaptureStatus.Success)
    {
      Navigation.NavigateTo("/");
    }
  }

  private void Close()
  {
    FaceCaptureState.Close();

    Navigation.NavigateTo("/");
  }

  public void Dispose()
  {
    FaceCaptureState.StateChanged -= OnStateChanged;
  }
}
