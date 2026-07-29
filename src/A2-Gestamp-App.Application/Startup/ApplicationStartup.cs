using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Application.Features.Inspection.IInspection;
using A2GestampApp.Domain.Features.Inspection.Models;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Application.Startup;

internal sealed class ApplicationStartup : IApplicationStartup
{
  private readonly IKeyenceService _keyenceService;
  private readonly IImageWatcherService _imageWatcher;
  private readonly IInspectionCoordinator _inspectionCoordinator;
  private readonly IImageTransferService _imageTransferService;
  private readonly IInspectionState _inspectionState;
  private readonly ILogger<ApplicationStartup> _logger;

  public ApplicationStartup(
    IKeyenceService keyenceService,
    IImageWatcherService imageWatcher,
    IInspectionCoordinator inspectionCoordinator,
    IInspectionState inspectionState,
    IImageTransferService imageTransferService,

    ILogger<ApplicationStartup> logger)
  {
    _keyenceService = keyenceService;
    _imageWatcher = imageWatcher;
    _inspectionCoordinator = inspectionCoordinator;
    _imageTransferService = imageTransferService;
    _inspectionState = inspectionState;
    _logger = logger;
  }

  public async Task StartAsync()
  {
    _keyenceService.InspectionReceived += _inspectionCoordinator.Process;
    _imageWatcher.ImageReceived += _inspectionCoordinator.Process;

    _inspectionCoordinator.InspectionCompleted += OnInspectionCompleted;

    _imageWatcher.Start();

    await _keyenceService.StartAsync();

    _logger.LogInformation("Application started.");
  }

  private void OnInspectionCompleted(Inspection inspection)
  {
    _imageTransferService.Transfer(inspection);

    _inspectionState.SetInspection(inspection);

    _logger.LogInformation("Inspeção concluída.");
  }
}
