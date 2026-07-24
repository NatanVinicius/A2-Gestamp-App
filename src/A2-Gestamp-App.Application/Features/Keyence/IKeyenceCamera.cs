namespace A2GestampApp.Application.Features.Keyence;

public interface IKeyenceCamera
{
  public int CameraId { get; }

  public bool IsConnected { get; }

  public event EventHandler<bool>? ConnectionStatusChanged;

  public event EventHandler<CameraInspection>? InspectionReceived;

  public Task ConnectAsync();

  public Task DisconnectAsync();

  public Task SimulateInspection(); // somente para o fake

  public event EventHandler<byte[]>? ImageReceived;
}
