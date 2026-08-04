using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Application.Features.Ng;
using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.Inspection.Enums;
using A2GestampApp.Domain.Features.ProductionShift.Entities;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Application.Startup;

internal sealed class ApplicationStartup : IApplicationStartup
{
  private readonly IKeyenceService _keyenceService;
  private readonly IProductionShiftRepository _productionShiftRepository;
  private readonly IInspectionRepository _inspectionRepository;
  private readonly IImageWatcherService _imageWatcher;
  private readonly IInspectionCoordinator _inspectionCoordinator;
  private readonly IImageTransferService _imageTransferService;
  private readonly IInspectionState _inspectionState;
  private readonly IProductionShiftState _productionShiftState;
  private readonly INgState _ngState;
  private readonly IFaceRecognitionService _faceRecognitionService;
  private readonly IAuthenticatedUserState _authenticatedUserState;
  private readonly ILogger<ApplicationStartup> _logger;

  public ApplicationStartup(
    IKeyenceService keyenceService,
    IProductionShiftRepository productionShiftRepository,
    IInspectionRepository inspectionRepository,
    IImageWatcherService imageWatcher,
    IInspectionCoordinator inspectionCoordinator,
    IInspectionState inspectionState,
    IProductionShiftState productionShiftState,
    INgState ngState,
    IFaceRecognitionService faceRecognitionService,
    IImageTransferService imageTransferService,
    IAuthenticatedUserState authenticatedUserState,
    ILogger<ApplicationStartup> logger)
  {
    _keyenceService = keyenceService;
    _productionShiftRepository = productionShiftRepository;
    _inspectionRepository = inspectionRepository;
    _imageWatcher = imageWatcher;
    _inspectionCoordinator = inspectionCoordinator;
    _imageTransferService = imageTransferService;
    _inspectionState = inspectionState;
    _productionShiftState = productionShiftState;
    _ngState = ngState;
    _faceRecognitionService = faceRecognitionService;
    _authenticatedUserState = authenticatedUserState;
    _logger = logger;

    _faceRecognitionService.UserRecognized += OnUserRecognized;
  }

  public async Task StartAsync()
  {
    ProductionShift? shift =
    await _productionShiftRepository.GetCurrentAsync();


    if (shift is null)
    {

      shift = ProductionShift.CreateCurrent();

      await _productionShiftRepository.AddAsync(shift);

    }

    _productionShiftState.SetCurrentShift(shift);

    _keyenceService.InspectionReceived += _inspectionCoordinator.Process;
    _imageWatcher.ImageReceived += _inspectionCoordinator.Process;

    _inspectionCoordinator.InspectionCompleted += OnInspectionCompleted;

    _imageWatcher.Start();

    await _keyenceService.StartAsync();

    await _faceRecognitionService.StartAsync();

    await _faceRecognitionService.DisableAsync();

    _logger.LogInformation("Application started.");
  }

  private async void OnInspectionCompleted(Inspection inspection)
  {
    try
    {
      _imageTransferService.Transfer(inspection);

      await EnsureCurrentShiftAsync();

      inspection.LinkToProductionShift(
          _productionShiftState.CurrentShift.Id);

      await _inspectionRepository.AddAsync(inspection);

      _productionShiftState.CurrentShift.RegisterInspection(
          inspection.FinalJudgement,
          inspection.CycleTime);

      await _productionShiftRepository.UpdateAsync(
          _productionShiftState.CurrentShift);

      _productionShiftState.NotifyStateChanged();

      _inspectionState.SetInspection(inspection);

      if (inspection.FinalJudgement == InspectionResult.Reprovada)
      {
        _ngState.Open();
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao persistir inspeção.");
      throw;
    }
  }

  private async void OnUserRecognized(FaceRecognitionEvent e)
  {
    _authenticatedUserState.SetUser(e);

    await _ngState.SetSuccessAsync();
  }

  private async Task EnsureCurrentShiftAsync()
  {

    ProductionShift currentShift =
        _productionShiftState.CurrentShift;

    _logger.LogInformation(
    "Agora: {Now} | Início: {Start} | Fim: {End} | Expirado: {Expired}",
    DateTime.Now,
    currentShift.StartDate,
    currentShift.EndDate,
    currentShift.IsExpired);

    if (!currentShift.IsExpired)
    {
      _logger.LogInformation("Turno ainda válido.");
      return;
    }

    _logger.LogInformation("Turno expirado. Criando novo turno.");

    currentShift.Close();

    await _productionShiftRepository.UpdateAsync(currentShift);

    ProductionShift newShift =
        ProductionShift.CreateCurrent();

    await _productionShiftRepository.AddAsync(newShift);

    _productionShiftState.SetCurrentShift(newShift);
  }
}
