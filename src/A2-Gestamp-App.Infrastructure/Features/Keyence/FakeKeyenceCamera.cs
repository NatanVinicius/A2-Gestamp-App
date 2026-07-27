using System.Diagnostics;

using A2GestampApp.Application.Features.Keyence;

using Features.Inspection.Domain;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Features.Keyence;

public sealed class FakeKeyenceCamera : IKeyenceCamera
{
  private readonly ILogger<FakeKeyenceCamera> _logger;

  public int CameraId { get; }

  public bool IsConnected { get; private set; }

  public event EventHandler<bool>? ConnectionStatusChanged;

  public event EventHandler<CameraInspection>? InspectionReceived;

  public event EventHandler<byte[]>? ImageReceived;

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

  public async Task SimulateInspection()
  {
    var inspection = new CameraInspection(
        cameraId: CameraId,
        passed: false,
        executionTime: 35.7,
        tools:
        [
            new ToolResult
            {
                Name = "Presence",
                Passed = true,
                ExecutionTime = 12.4
            },

            new ToolResult
            {
                Name = "Diameter",
                Passed = true,
                ExecutionTime = 10.8
            },

            new ToolResult
            {
                Name = "Position",
                Passed = true,
                ExecutionTime = 12.5
            }
        ]);

    InspectionReceived?.Invoke(this, inspection);

    Debug.WriteLine($"[FAKE] Antes do delay {CameraId}");

    await Task.Delay(300);

    Debug.WriteLine($"[FAKE] Disparando imagem {CameraId}");

    ImageReceived?.Invoke(this, CreateFakeImage());
  }

  private static byte[] CreateFakeImage()
  {
    return new byte[100];
  }
}
