using A2GestampApp.Application.Features.Keyence;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Features.Keyence;

public sealed class FakeKeyenceCamera : IKeyenceCamera
{
  private readonly ILogger<FakeKeyenceCamera> _logger;

  public int CameraId { get; }

  public bool IsConnected { get; private set; }

  public event EventHandler<bool>? ConnectionStatusChanged;

  public FakeKeyenceCamera(
      int cameraId,
      ILogger<FakeKeyenceCamera> logger)
  {
    CameraId = cameraId;
    _logger = logger;
  }

  public Task ConnectAsync()
  {
    if (IsConnected)
    {
      _logger.LogInformation("Camera {CameraId} is already connected.", CameraId);
      return Task.CompletedTask;
    }

    _logger.LogInformation("Connecting camera {CameraId}...", CameraId);

    SetConnectionState(true);

    _logger.LogInformation("Camera {CameraId} connected.", CameraId);

    return Task.CompletedTask;
  }

  public Task DisconnectAsync()
  {
    if (!IsConnected)
    {
      _logger.LogInformation("Camera {CameraId} is already disconnected.", CameraId);
      return Task.CompletedTask;
    }

    _logger.LogInformation("Disconnecting camera {CameraId}...", CameraId);

    SetConnectionState(false);

    _logger.LogInformation("Camera {CameraId} disconnected.", CameraId);

    return Task.CompletedTask;
  }

  private void SetConnectionState(bool connected)
  {
    if (IsConnected == connected)
    {
      return;
    }

    IsConnected = connected;

    ConnectionStatusChanged?.Invoke(this, connected);
  }
}
