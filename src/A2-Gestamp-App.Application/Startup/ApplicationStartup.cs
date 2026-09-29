using A2GestampApp.Application.Features.AdminAuthentication;
using A2GestampApp.Application.Features.Hikvision;
using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Application.Features.Ng;
using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.Inspection.Enums;
using A2GestampApp.Domain.Features.ProductionShift.Entities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace A2GestampApp.Application.Startup;

internal sealed class ApplicationStartup : IApplicationStartup
{
  private readonly IKeyenceService _keyenceService;
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
  private readonly IServiceScopeFactory _serviceScopeFactory;
  private readonly SemaphoreSlim _inspectionProcessingLock = new(1, 1);

  public ApplicationStartup(
    IKeyenceService keyenceService,
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
    ILogger<ApplicationStartup> logger,
    IServiceScopeFactory serviceScopeFactory)
  {
    _keyenceService = keyenceService;
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
    _serviceScopeFactory = serviceScopeFactory;

    _faceRecognitionService.UserRecognized += OnUserRecognized;
  }

  public async Task StartAsync()
  {
    _logger.LogInformation(
        "[Application] Starting application.");

    ProductionShift? shift;

    using (IServiceScope scope = _serviceScopeFactory.CreateScope())
    {
      IProductionShiftRepository productionShiftRepository =
          scope.ServiceProvider.GetRequiredService<IProductionShiftRepository>();

      shift = await productionShiftRepository.GetCurrentAsync();

      if (shift is null)
      {
        _logger.LogInformation(
            "[Application] No current production shift found. Creating new shift.");

        shift = ProductionShift.CreateCurrent();

        await productionShiftRepository.AddAsync(shift);

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
    }

    _productionShiftState.SetCurrentShift(shift);

    _keyenceService.InspectionReceived += _inspectionCoordinator.Process;
    _imageWatcher.ImageReceived += _inspectionCoordinator.Process;

    _inspectionCoordinator.InspectionCompleted += OnInspectionCompleted;

    _logger.LogDebug(
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

    _logger.LogDebug(
        "[Application] Image watcher started.");

    await _keyenceService.StartAsync();

    _logger.LogDebug(
        "[Application] Keyence service started.");

    try
    {
      await _faceRecognitionService.StartAsync();

      _logger.LogDebug(
          "[Application] Face recognition service started.");
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Application] Unable to connect to face recognition service.");
    }

    try
    {
      await _faceImageServer.StartAsync();

      _logger.LogDebug(
          "[Application] Face image server started.");
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Application] Unable to connect to face image server.");
    }

    try
    {
      await _faceRecognitionService.DisableAsync();

      _logger.LogDebug(
          "[Application] Face recognition service disabled.");
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Application] Unable to disable face recognition service.");
    }

    _logger.LogInformation(
        "[Application] Application started.");
  }

  private async void OnInspectionCompleted(
      Inspection inspection)
  {
    await _inspectionProcessingLock.WaitAsync();

    try
    {
      _logger.LogInformation(
          "[Application] Inspection completed. Result: {Result}, CycleTime: {CycleTime}",
          inspection.FinalJudgement,
          inspection.CycleTime);

      _imageTransferService.Transfer(inspection);

      await EnsureCurrentShiftAsync();

      int shiftId = _productionShiftState.CurrentShift.Id;

      inspection.LinkToProductionShift(shiftId);

      using (IServiceScope scope = _serviceScopeFactory.CreateScope())
      {
        IInspectionRepository inspectionRepository =
            scope.ServiceProvider.GetRequiredService<IInspectionRepository>();

        await inspectionRepository.AddAsync(inspection);
      }

      _logger.LogInformation(
          "[Application] Inspection persisted. InspectionId: {InspectionId}, ShiftId: {ShiftId}",
          inspection.Id,
          shiftId);

      _productionShiftState.CurrentShift.RegisterInspection(
          inspection.FinalJudgement,
          inspection.CycleTime);

      using (IServiceScope scope = _serviceScopeFactory.CreateScope())
      {
        IProductionShiftRepository productionShiftRepository =
            scope.ServiceProvider.GetRequiredService<IProductionShiftRepository>();

        await productionShiftRepository.UpdateAsync(
            _productionShiftState.CurrentShift);
      }

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
          "[Application] Error processing completed inspection. InspectionId: {InspectionId}",
          inspection.Id);
    }
    finally
    {
      _inspectionProcessingLock.Release();
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

    using IServiceScope scope = _serviceScopeFactory.CreateScope();

    IProductionShiftRepository productionShiftRepository =
        scope.ServiceProvider.GetRequiredService<IProductionShiftRepository>();

    currentShift.Close();

    await productionShiftRepository.UpdateAsync(
        currentShift);

    ProductionShift newShift =
        ProductionShift.CreateCurrent();

    await productionShiftRepository.AddAsync(
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
