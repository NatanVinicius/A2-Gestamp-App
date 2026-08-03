using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Application.Features.Ng;
using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.Inspection.Enums;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Application.Startup;

internal sealed class ApplicationStartup : IApplicationStartup
{
  private readonly IKeyenceService _keyenceService;
  private readonly IImageWatcherService _imageWatcher;
  private readonly IInspectionCoordinator _inspectionCoordinator;
  private readonly IImageTransferService _imageTransferService;
  private readonly IInspectionState _inspectionState;
  private readonly IInspectionStatisticsState _inspectionStatisticsState;
  private readonly InspectionStatistics _statistics = new();
  private readonly INgState _ngState;
  private readonly IFaceRecognitionService _faceRecognitionService;
  private readonly IAuthenticatedUserState _authenticatedUserState;
  private readonly ILogger<ApplicationStartup> _logger;

  public ApplicationStartup(
    IKeyenceService keyenceService,
    IImageWatcherService imageWatcher,
    IInspectionCoordinator inspectionCoordinator,
    IInspectionState inspectionState,
    IInspectionStatisticsState inspectionStatisticsState,
    INgState ngState,
    IFaceRecognitionService faceRecognitionService,
    IImageTransferService imageTransferService,
    IAuthenticatedUserState authenticatedUserState,
    ILogger<ApplicationStartup> logger)
  {
    _keyenceService = keyenceService;
    _imageWatcher = imageWatcher;
    _inspectionCoordinator = inspectionCoordinator;
    _imageTransferService = imageTransferService;
    _inspectionState = inspectionState;
    _ngState = ngState;
    _inspectionStatisticsState = inspectionStatisticsState;
    _faceRecognitionService = faceRecognitionService;
    _authenticatedUserState = authenticatedUserState;
    _logger = logger;

    _faceRecognitionService.UserRecognized += OnUserRecognized;
  }

  public async Task StartAsync()
  {
    _keyenceService.InspectionReceived += _inspectionCoordinator.Process;
    _imageWatcher.ImageReceived += _inspectionCoordinator.Process;

    _inspectionCoordinator.InspectionCompleted += OnInspectionCompleted;

    _imageWatcher.Start();

    await _keyenceService.StartAsync();

    await _faceRecognitionService.StartAsync();

    await _faceRecognitionService.DisableAsync();

    _logger.LogInformation("Application started.");
  }

  private void OnInspectionCompleted(Inspection inspection)
  {
    _imageTransferService.Transfer(inspection);

    _statistics.Register(inspection);

    _inspectionStatisticsState.SetStatistics(_statistics);

    _inspectionState.SetInspection(inspection);

    if (inspection.Result == InspectionResult.Reprovada)
    {
      _ngState.Open();
    }

    _logger.LogInformation("Inspeção concluída.");
  }

  private async void OnUserRecognized(FaceRecognitionEvent e)
  {
    _authenticatedUserState.SetUser(e);

    await _ngState.SetSuccessAsync();
  }
}
