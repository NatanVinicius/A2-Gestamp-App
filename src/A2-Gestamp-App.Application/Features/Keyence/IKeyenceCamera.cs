namespace A2GestampApp.Application.Features.Keyence;

public interface IKeyenceCamera
{
  public int CameraId { get; }

  public bool IsConnected { get; }

  public event EventHandler<bool>? ConnectionStatusChanged;

  public Task ConnectAsync();

  public Task DisconnectAsync();
}
