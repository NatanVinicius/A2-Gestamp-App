using System.Net;
using System.Text.Json;

using A2GestampApp.Infrastructure.Hikvision.Models;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class HikvisionFaceRecognitionService : IFaceRecognitionService
{
  private readonly FaceRecognitionServer _server;
  private readonly HikvisionClient _client;

  private const string DeviceAddress = "192.168.70.40";

  private bool _started;

  public event Action<FaceRecognitionEvent>? UserRecognized;

  public HikvisionFaceRecognitionService(
      HikvisionClient client,
      FaceRecognitionServer server)
  {
    _client = client;
    _server = server;

    _server.RequestReceived += OnRequestReceived;
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

    var accessEvent = hikvisionEvent.AccessControllerEvent;

    if (accessEvent.SubEventType != 75)
    {
      return;
    }

    var employeeNumber = accessEvent.EmployeeNoString;

    if (string.IsNullOrWhiteSpace(employeeNumber))
    {
      return;
    }

    var user = await _client.SearchUserAsync(
        DeviceAddress,
        employeeNumber);

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

  public Task EnableAsync()
  {
    return _client.SetCardReaderEnabledAsync(true);
  }

  public Task DisableAsync()
  {
    return _client.SetCardReaderEnabledAsync(false);
  }
}
