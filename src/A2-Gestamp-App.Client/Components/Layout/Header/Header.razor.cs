using A2GestampApp.Application.Features.Keyence;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Layout.Header;

public partial class Header : ComponentBase, IDisposable
{
  [Inject]
  private IEnumerable<IKeyenceCamera> Cameras { get; set; } = default!;

  protected IEnumerable<IKeyenceCamera> OrderedCameras =>
      Cameras.OrderBy(c => c.CameraId);

  protected bool IsCommunicationModalOpen;

  protected string CurrentTime =>
      DateTime.Now.ToString("HH:mm:ss");

  protected string CurrentDate =>
      DateTime.Now.ToString("dd/MM/yyyy");

  protected override void OnInitialized()
  {
    foreach (var camera in Cameras)
    {
      camera.ConnectionStatusChanged += OnConnectionChanged;
    }
  }

  private void OnConnectionChanged(object? sender, bool connected)
  {
    InvokeAsync(StateHasChanged);
  }

  protected void OpenCommunicationModal()
  {
    IsCommunicationModalOpen = true;
  }

  protected string GetStatusColor(bool connected)
      => connected
          ? "bg-green-500"
          : "bg-red-500";

  public void Dispose()
  {
    foreach (var camera in Cameras)
    {
      camera.ConnectionStatusChanged -= OnConnectionChanged;
    }
  }
}
