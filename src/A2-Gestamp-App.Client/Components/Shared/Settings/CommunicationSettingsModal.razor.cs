using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace A2GestampApp.Client.Components.Shared.Settings;

public partial class CommunicationSettingsModal
{
  [Inject]
  private INetworkConnectionService NetworkConnectionService { get; set; } = default!;

  [Inject]
  private IOptions<NetworkSettings> NetworkSettings { get; set; } = default!;

  [Parameter]
  public bool IsOpen { get; set; }

  [Parameter]
  public EventCallback<bool> IsOpenChanged { get; set; }

  private bool? Camera1Online;
  private bool? Camera2Online;
  private bool? Camera3Online;
  private bool? PlcOnline;
  private bool? HostOnline;
  private bool IsLoading = false;

  private async Task Close()
  {
    ResetConnections();
    await IsOpenChanged.InvokeAsync(false);
  }

  private async Task TestConnections()
  {
    try
    {
      IsLoading = true;

      var settings = NetworkSettings.Value;

      Camera1Online = await NetworkConnectionService.PingAsync(settings.Camera1);
      Camera2Online = await NetworkConnectionService.PingAsync(settings.Camera2);
      Camera3Online = await NetworkConnectionService.PingAsync(settings.Camera3);

      PlcOnline = await NetworkConnectionService.PingAsync(settings.Plc);
      HostOnline = await NetworkConnectionService.PingAsync(settings.Host);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
    }
    finally
    {
      IsLoading = false;
    }
  }

  private static string GetStatusColor(bool? status)
  {
    return status switch
    {
      true => "text-green-500",
      false => "text-red-500",
      _ => "text-white/30"
    };
  }

  private void ResetConnections()
  {
    Camera1Online = null;
    Camera2Online = null;
    Camera3Online = null;

    PlcOnline = null;
    HostOnline = null;
    IsLoading = false;
  }
}
