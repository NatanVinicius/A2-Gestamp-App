using A2GestampApp.Application.Features.Keyence;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Application.Startup;

internal sealed class ApplicationStartup : IApplicationStartup
{
  private readonly IEnumerable<IKeyenceCamera> _cameras;
  private readonly ILogger<ApplicationStartup> _logger;

  public ApplicationStartup(
      IEnumerable<IKeyenceCamera> cameras,
      ILogger<ApplicationStartup> logger)
  {
    _cameras = cameras;
    _logger = logger;
  }

  public async Task StartAsync()
  {
    foreach (IKeyenceCamera camera in _cameras.OrderBy(c => c.CameraId))
    {
      await camera.ConnectAsync();

      await camera.SimulateInspection();
    }

    _logger.LogInformation("Application started.");
  }
}
