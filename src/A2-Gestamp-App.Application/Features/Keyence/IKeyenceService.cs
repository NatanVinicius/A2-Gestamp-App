using A2GestampApp.Application.Features.Keyence.Models;

public interface IKeyenceService
{
  public event Action<CameraInspectionResult> InspectionReceived;

  public Task StartAsync(CancellationToken cancellationToken = default);

  public Task StopAsync();
}
