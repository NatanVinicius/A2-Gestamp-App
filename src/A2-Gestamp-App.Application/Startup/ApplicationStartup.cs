using A2GestampApp.Application.Features.AdminAuthentication;
using A2GestampApp.Application.Features.Hikvision;
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
  private readonly IPlcService _plcService;
  private readonly IAdminAuthenticationState _adminAuthenticationState;
  private readonly IFaceImageServer _faceImageServer;
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
    IPlcService plcService,
    IAdminAuthenticationState adminAuthenticationState,
    IFaceImageServer faceImageServer,
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
    _plcService = plcService;
    _adminAuthenticationState = adminAuthenticationState;
    _faceImageServer = faceImageServer;
    _logger = logger;

    _faceRecognitionService.UserRecognized += OnUserRecognized;
  }

  public async Task StartAsync()
  {
    _logger.LogInformation(
        "[Application] Starting application.");

    ProductionShift? shift =
        await _productionShiftRepository.GetCurrentAsync();

    if (shift is null)
    {
      _logger.LogInformation(
          "[Application] No current production shift found. Creating new shift.");

      shift = ProductionShift.CreateCurrent();

      await _productionShiftRepository.AddAsync(shift);

      _logger.LogInformation(
          "[Application] Production shift created. Start: {Start}, End: {End}, Shift: {Shift}",
          shift.StartDate,
          shift.EndDate,
          shift.ShiftNumber);
    }
    else
    {
      _logger.LogInformation(
          "[Application] Current production shift loaded. Id: {Id}, Shift: {Shift}, Start: {Start}, End: {End}",
          shift.Id,
          shift.ShiftNumber,
          shift.StartDate,
          shift.EndDate);
    }

    _productionShiftState.SetCurrentShift(shift);

    _keyenceService.InspectionReceived += _inspectionCoordinator.Process;
    _imageWatcher.ImageReceived += _inspectionCoordinator.Process;

    _inspectionCoordinator.InspectionCompleted += OnInspectionCompleted;

    _logger.LogInformation(
        "[Application] Inspection event handlers registered.");

    try
    {
      await _plcService.ConnectAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Application] Unable to connect to PLC.");
    }

    _imageWatcher.Start();

    _logger.LogInformation(
        "[Application] Image watcher started.");

    await _keyenceService.StartAsync();

    _logger.LogInformation(
        "[Application] Keyence service started.");

    await _faceRecognitionService.StartAsync();

    _logger.LogInformation(
        "[Application] Face recognition service started.");

    await _faceImageServer.StartAsync();

    _logger.LogInformation(
        "[Application] Face image server started.");

    await _faceRecognitionService.DisableAsync();

    _logger.LogInformation(
        "[Application] Face recognition disabled after startup.");



    _logger.LogInformation(
        "[Application] Application started.");
  }

  private async void OnInspectionCompleted(
      Inspection inspection)
  {
    try
    {
      _logger.LogInformation(
          "[Application] Inspection completed. Result: {Result}, CycleTime: {CycleTime}",
          inspection.FinalJudgement,
          inspection.CycleTime);

      _imageTransferService.Transfer(inspection);

      await EnsureCurrentShiftAsync();

      inspection.LinkToProductionShift(
          _productionShiftState.CurrentShift.Id);

      await _inspectionRepository.AddAsync(inspection);

      _logger.LogInformation(
          "[Application] Inspection persisted. InspectionId: {InspectionId}, ShiftId: {ShiftId}",
          inspection.Id,
          _productionShiftState.CurrentShift.Id);

      _productionShiftState.CurrentShift.RegisterInspection(
          inspection.FinalJudgement,
          inspection.CycleTime);

      await _productionShiftRepository.UpdateAsync(
          _productionShiftState.CurrentShift);

      _productionShiftState.NotifyStateChanged();

      if (inspection.FinalJudgement == InspectionResult.Aprovada)
      {
        _logger.LogInformation(
            "[Application] Inspection approved. Sending PLC Approved signal.");

        await _plcService.WriteAsync(
            PlcRegisters.Approved,
            1);
      }
      else
      {
        _logger.LogInformation(
            "[Application] Inspection rejected. Sending PLC Rejected signal.");

        await _plcService.WriteAsync(
            PlcRegisters.Rejected,
            1);
      }

      _inspectionState.SetInspection(inspection);

      if (inspection.FinalJudgement == InspectionResult.Reprovada)
      {
        _logger.LogInformation(
            "[Application] NG inspection detected. Opening NG state.");

        _ngState.Open();
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Application] Error processing completed inspection.");

      throw;
    }
  }

  private async void OnUserRecognized(
      FaceRecognitionEvent e)
  {
    _logger.LogInformation(
        "[Application] User recognized. EmployeeNumber: {EmployeeNumber}, Name: {Name}, Role: {Role}",
        e.EmployeeNumber,
        e.Name,
        e.Role);

    _authenticatedUserState.SetUser(e);

    if (_ngState.IsOpen)
    {
      _logger.LogInformation(
          "[Application] NG state is open. Authenticating user for NG release.");

      await _ngState.SetSuccessAsync();

      return;
    }

    if (_adminAuthenticationState.IsOpen)
    {
      _logger.LogInformation(
          "[Application] Admin authentication is open. Authenticating recognized user.");

      await _adminAuthenticationState.AuthenticateAsync(e.Role);

      return;
    }
  }

  private async Task EnsureCurrentShiftAsync()
  {
    ProductionShift currentShift =
        _productionShiftState.CurrentShift;

    _logger.LogInformation(
        "[Application] Checking current shift. Now: {Now}, Start: {Start}, End: {End}, Expired: {Expired}",
        DateTime.Now,
        currentShift.StartDate,
        currentShift.EndDate,
        currentShift.IsExpired);

    if (!currentShift.IsExpired)
    {
      return;
    }

    _logger.LogInformation(
        "[Application] Current shift expired. Closing and creating new shift.");

    currentShift.Close();

    await _productionShiftRepository.UpdateAsync(
        currentShift);

    ProductionShift newShift =
        ProductionShift.CreateCurrent();

    await _productionShiftRepository.AddAsync(
        newShift);

    _productionShiftState.SetCurrentShift(
        newShift);

    _logger.LogInformation(
        "[Application] New production shift created. Id: {Id}, Shift: {Shift}, Start: {Start}, End: {End}",
        newShift.Id,
        newShift.ShiftNumber,
        newShift.StartDate,
        newShift.EndDate);
  }

  public async Task StopAsync()
  {
    _logger.LogInformation(
        "[Application] Stopping application.");

    try
    {
      await _plcService.DisconnectAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Application] Error disconnecting PLC.");
    }

    _logger.LogInformation(
        "[Application] Application stopped.");
  }
}
