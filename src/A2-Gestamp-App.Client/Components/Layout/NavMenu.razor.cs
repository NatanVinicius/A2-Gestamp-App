using A2GestampApp.Application.Features.AdminAuthentication;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Layout;

public partial class NavMenu
{
  [Inject]
  public required IAdminAuthenticationState AdminAuthenticationState { get; init; }


  private void AuthenticateAdminAsync()
  {
    AdminAuthenticationState.Open();
  }
}
