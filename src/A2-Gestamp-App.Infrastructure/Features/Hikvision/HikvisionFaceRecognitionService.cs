using System.Net;
using System.Text.Json;

using A2GestampApp.Application.Features.System;
using A2GestampApp.Infrastructure.Hikvision.Models;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class HikvisionFaceRecognitionService : IFaceRecognitionService
{
  private readonly FaceRecognitionServer _server;
  private readonly HikvisionClient _client;
  private readonly ISystemState _systemState;

  private const string DeviceAddress = "192.168.70.40";

  private bool _started;

  public event Action<FaceRecognitionEvent>? UserRecognized;

  public HikvisionFaceRecognitionService(
    HikvisionClient client,
    FaceRecognitionServer server,
    ISystemState systemState)
  {
    _client = client;
    _server = server;
    _systemState = systemState;

    _server.RequestReceived += OnRequestReceived;
    _server.Started += OnServerStarted;
    _server.Stopped += OnServerStopped;
  }

  public async Task StartAsync()
  {
    if (_started)
    {
      return;
    }

    _started = true;

    await _server.StartAsync(7333);

    await _client.RegisterHttpHostAsync(
        deviceAddress: DeviceAddress,
        platformIp: "192.168.70.35",
        platformPort: 7333);
  }

  private async Task OnRequestReceived(HttpListenerRequest request)
  {
    try
    {
      using var reader = new StreamReader(request.InputStream);

      var body = await reader.ReadToEndAsync();

      var json = HikvisionMultipartParser.ExtractEventLog(body);

      if (json is null)
      {
        return;
      }

      var hikvisionEvent =
          JsonSerializer.Deserialize<HikvisionEvent>(json);

      if (hikvisionEvent is null)
      {
        return;
      }

      if (hikvisionEvent.AccessControllerEvent is null)
      {
        return;
      }

      var accessEvent = hikvisionEvent.AccessControllerEvent;

      if (string.IsNullOrWhiteSpace(accessEvent.EmployeeNoString))
      {
        return;
      }

      var user = await _client.SearchUserAsync(
          DeviceAddress,
          accessEvent.EmployeeNoString);

      if (user is null)
      {
        return;
      }

      await DisableAsync();

      UserRecognized?.Invoke(new FaceRecognitionEvent
      {
        EmployeeNumber = user.EmployeeNumber,
        Name = user.Name,
        Role = user.Role
      });
    }
    catch (Exception)
    {
      System.Diagnostics.Debugger.Break();
      throw;
    }
  }

  public Task EnableAsync()
  {
    return _client.SetCardReaderEnabledAsync(true);
  }

  public Task DisableAsync()
  {
    return _client.SetCardReaderEnabledAsync(false);
  }

  private void OnServerStarted()
  {
    _systemState.SetHikvisionStatus(
        CommunicationStatus.Connected);
  }

  private void OnServerStopped()
  {
    _systemState.SetHikvisionStatus(
        CommunicationStatus.Disconnected);
  }

  public void Dispose()
  {
    _server.RequestReceived -= OnRequestReceived;
    _server.Started -= OnServerStarted;
    _server.Stopped -= OnServerStopped;
  }
}
