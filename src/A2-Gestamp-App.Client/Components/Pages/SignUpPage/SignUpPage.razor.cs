using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.SignUpPage;

public partial class SignUpPage : ComponentBase,
      IDisposable
{
  [Inject]
  public required ISignUpState SignUpState { get; init; }

  [Inject]
  public required IFaceCaptureState FaceCaptureState { get; init; }


  [Inject]
  public required IAuthenticatedUserState AuthenticatedUserState { get; init; }

  protected override void OnInitialized()
  {
    SignUpState.Reset();
  }

  private void OnRegister()
  {
    if (string.IsNullOrWhiteSpace(SignUpState.Name))
    {
      return;
    }

    FaceCaptureState.Open();
  }

  public void Dispose()
  {
    SignUpState.Reset();

    AuthenticatedUserState.Clear();
  }
}
