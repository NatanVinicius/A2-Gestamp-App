using System.Net;
using System.Text.Json;

using A2GestampApp.Application.Features.System;
using A2GestampApp.Infrastructure.Hikvision.Models;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class HikvisionFaceRecognitionService
    : IFaceRecognitionService
{
  private readonly FaceRecognitionServer _server;
  private readonly HikvisionClient _client;
  private readonly ISystemState _systemState;
  private readonly ILogger<HikvisionFaceRecognitionService> _logger;

  private const string DeviceAddress = "192.168.70.40";

  private bool _started;

  public event Action<FaceRecognitionEvent>? UserRecognized;

  public HikvisionFaceRecognitionService(
      HikvisionClient client,
      FaceRecognitionServer server,
      ISystemState systemState,
      ILogger<HikvisionFaceRecognitionService> logger)
  {
    _client = client;
    _server = server;
    _systemState = systemState;
    _logger = logger;

    _server.RequestReceived += OnRequestReceived;
    _server.Started += OnServerStarted;
    _server.Stopped += OnServerStopped;
  }

  public async Task StartAsync()
  {
    if (_started)
    {
      _logger.LogInformation(
          "[FaceRecognition] Service already started.");

      return;
    }

    _logger.LogInformation(
        "[FaceRecognition] Starting service.");

    _started = true;

    await _server.StartAsync(7333);

    _logger.LogInformation(
        "[FaceRecognition] Recognition server started on port 7333.");

    await _client.RegisterHttpHostAsync(
        deviceAddress: DeviceAddress,
        platformIp: "192.168.70.35",
        platformPort: 7333);

    _logger.LogInformation(
        "[FaceRecognition] Hikvision HTTP host registered successfully.");
  }

  private async Task OnRequestReceived(
      HttpListenerRequest request)
  {
    try
    {
      _logger.LogInformation(
          "[FaceRecognition] Event received. Method: {Method}, Path: {Path}, Remote: {Remote}",
          request.HttpMethod,
          request.Url?.AbsolutePath,
          request.RemoteEndPoint);

      using var reader =
          new StreamReader(request.InputStream);

      var body =
          await reader.ReadToEndAsync();

      _logger.LogInformation(
          "[FaceRecognition] Event payload received. Size: {Size} bytes.",
          body.Length);

      var json =
          HikvisionMultipartParser.ExtractEventLog(body);

      if (json is null)
      {
        _logger.LogWarning(
            "[FaceRecognition] Event log could not be extracted from request.");

        return;
      }

      var hikvisionEvent =
          JsonSerializer.Deserialize<HikvisionEvent>(json);

      if (hikvisionEvent is null)
      {
        _logger.LogWarning(
            "[FaceRecognition] Hikvision event JSON could not be deserialized.");

        return;
      }

      if (hikvisionEvent.AccessControllerEvent is null)
      {
        _logger.LogWarning(
            "[FaceRecognition] Event does not contain AccessControllerEvent.");

        return;
      }

      var accessEvent =
          hikvisionEvent.AccessControllerEvent;

      if (string.IsNullOrWhiteSpace(
              accessEvent.EmployeeNoString))
      {
        _logger.LogWarning(
            "[FaceRecognition] Event does not contain EmployeeNo.");

        return;
      }

      var employeeNumber =
          accessEvent.EmployeeNoString;

      _logger.LogInformation(
          "[FaceRecognition] EmployeeNo received: {EmployeeNumber}",
          employeeNumber);

      var user =
          await _client.SearchUserAsync(
              DeviceAddress,
              employeeNumber);

      if (user is null)
      {
        _logger.LogWarning(
            "[FaceRecognition] User not found. EmployeeNo: {EmployeeNumber}",
            employeeNumber);

        return;
      }

      _logger.LogInformation(
          "[FaceRecognition] User recognized. EmployeeNo: {EmployeeNumber}, Name: {Name}, Role: {Role}",
          user.EmployeeNumber,
          user.Name,
          user.Role);

      await DisableAsync();

      _logger.LogInformation(
          "[FaceRecognition] Card reader disabled after recognition.");

      UserRecognized?.Invoke(
          new FaceRecognitionEvent
          {
            EmployeeNumber = user.EmployeeNumber,
            Name = user.Name,
            Role = user.Role
          });

      _logger.LogInformation(
          "[FaceRecognition] UserRecognized event dispatched. EmployeeNo: {EmployeeNumber}",
          user.EmployeeNumber);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceRecognition] Error processing Hikvision recognition event.");

      throw;
    }
  }

  public async Task EnableAsync()
  {
    _logger.LogInformation(
        "[FaceRecognition] Enabling card reader.");

    try
    {
      await _client.SetCardReaderEnabledAsync(true);

      _logger.LogInformation(
          "[FaceRecognition] Card reader enabled successfully.");
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceRecognition] Failed to enable card reader.");

      throw;
    }
  }

  public async Task DisableAsync()
  {
    _logger.LogInformation(
        "[FaceRecognition] Disabling card reader.");

    try
    {
      await _client.SetCardReaderEnabledAsync(false);

      _logger.LogInformation(
          "[FaceRecognition] Card reader disabled successfully.");
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceRecognition] Failed to disable card reader.");

      throw;
    }
  }

  private void OnServerStarted()
  {
    _logger.LogInformation(
        "[FaceRecognition] HTTP recognition server started.");

    _systemState.SetHikvisionStatus(
        CommunicationStatus.Connected);
  }

  private void OnServerStopped()
  {
    _logger.LogWarning(
        "[FaceRecognition] HTTP recognition server stopped.");

    _systemState.SetHikvisionStatus(
        CommunicationStatus.Disconnected);
  }

  public void Dispose()
  {
    _logger.LogInformation(
        "[FaceRecognition] Disposing service.");

    _server.RequestReceived -= OnRequestReceived;
    _server.Started -= OnServerStarted;
    _server.Stopped -= OnServerStopped;
  }
}
