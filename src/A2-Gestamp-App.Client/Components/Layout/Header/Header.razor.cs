using System.Timers;

using A2GestampApp.Application.Features.AdminAuthentication;
using A2GestampApp.Application.Features.System;

using Microsoft.AspNetCore.Components;

using Timer = System.Timers.Timer;

namespace A2GestampApp.Client.Components.Layout.Header;

public partial class Header : ComponentBase, IDisposable
{
  private readonly Timer _timer = new(1000);

  [Inject]
  protected ISystemState SystemState { get; set; } = default!;

  [Inject]
  protected IAdminAuthenticationState AdminAuthenticationState { get; set; } = default!;

  protected bool IsCommunicationModalOpen;

  protected bool IsSkuConfigurationModalOpen;

  protected string CurrentTime =>
      DateTime.Now.ToString("HH:mm:ss");

  protected string CurrentDate =>
      DateTime.Now.ToString("dd/MM/yyyy");

  protected override void OnInitialized()
  {
    _timer.Elapsed += OnTimerElapsed;
    _timer.Start();

    SystemState.StateChanged += OnSystemStateChanged;
    AdminAuthenticationState.StateChanged += OnAdminAuthenticationStateChanged;
  }

  private void OnTimerElapsed(
      object? sender,
      ElapsedEventArgs e)
  {
    InvokeAsync(StateHasChanged);
  }

  private void OnSystemStateChanged()
  {
    InvokeAsync(StateHasChanged);
  }

  private void OnAdminAuthenticationStateChanged()
  {
    if (AdminAuthenticationState.Status ==
        FaceRecognitionStatus.Success &&
        AdminAuthenticationState.Purpose ==
        AdminAuthenticationPurpose.Sku)
    {
      IsSkuConfigurationModalOpen = true;
    }

    _ = InvokeAsync(StateHasChanged);
  }

  protected void OpenCommunicationModal()
  {
    IsCommunicationModalOpen = true;
  }

  protected void OpenSkuConfiguration()
  {
    //AdminAuthenticationState.Open(
    //AdminAuthenticationPurpose.Sku);
    IsSkuConfigurationModalOpen = true;
  }

  protected string GetStatusColor(
      CommunicationStatus status)
  {
    return status == CommunicationStatus.Connected
        ? "bg-green-500"
        : "bg-red-500";
  }

  public void Dispose()
  {
    _timer.Elapsed -= OnTimerElapsed;
    _timer.Dispose();

    SystemState.StateChanged -= OnSystemStateChanged;
    AdminAuthenticationState.StateChanged -= OnAdminAuthenticationStateChanged;
  }
}
