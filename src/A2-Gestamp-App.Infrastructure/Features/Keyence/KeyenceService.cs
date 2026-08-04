using A2GestampApp.Application.Features.Keyence.Models;
using A2GestampApp.Application.Features.System;

using Infrastructure.Features.Keyence;
using Infrastructure.Features.Keyence.Parsers;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Features.Keyence;

public sealed class KeyenceService : IKeyenceService
{
  private readonly KeyenceOptions _options;
  private readonly ILogger<KeyenceService> _logger;
  private readonly ILogger<KeyenceTcpConnection> _connectionLogger;

  private readonly List<KeyenceTcpConnection> _connections = [];

  private readonly CameraMessageParser _parser;

  public event Action<CameraInspectionResult>? InspectionReceived;

  private readonly ISystemState _systemState;

  public KeyenceService(
    KeyenceOptions options,
    CameraMessageParser parser,
    ILogger<KeyenceService> logger,
    ISystemState systemState,
    ILogger<KeyenceTcpConnection> connectionLogger)
  {
    _options = options;
    _logger = logger;
    _connectionLogger = connectionLogger;
    _systemState = systemState;
    _parser = parser;


  }

  public async Task StartAsync(
    CancellationToken cancellationToken = default)
  {
    foreach (var camera in _options.Cameras)
    {
      var connection = new KeyenceTcpConnection(
          camera,
          _connectionLogger);

      connection.MessageReceived += OnMessageReceived;
      connection.Connected += OnConnected;
      connection.Disconnected += OnDisconnected;

      _connections.Add(connection);

      await connection.ConnectAsync(cancellationToken);
    }

    _logger.LogInformation(
        "Keyence service started.");
  }

  public async Task StopAsync()
  {
    foreach (var connection in _connections)
    {
      connection.MessageReceived -= OnMessageReceived;
      connection.Connected -= OnConnected;
      connection.Disconnected -= OnDisconnected;

      await connection.DisconnectAsync();

      connection.Dispose();
    }

    _connections.Clear();

    _logger.LogInformation(
        "Keyence service stopped.");
  }

  private void OnMessageReceived(CameraMessage message)
  {
    var inspection = _parser.Parse(message.RawMessage);

    InspectionReceived?.Invoke(inspection);
  }

  private void OnConnected(KeyenceTcpConnection connection)
  {
    switch (connection.Camera.Name)
    {
      case "VS1":
        _systemState.SetCamera1Status(CommunicationStatus.Connected);
        break;

      case "VS2":
        _systemState.SetCamera2Status(CommunicationStatus.Connected);
        break;

      case "VS3":
        _systemState.SetCamera3Status(CommunicationStatus.Connected);
        break;
    }
  }

  private void OnDisconnected(KeyenceTcpConnection connection)
  {
    _logger.LogInformation(
        "OnConnected disparado para {Camera}",
        connection.Camera.Name);

    switch (connection.Camera.Name)
    {
      case "VS1":
        _systemState.SetCamera1Status(CommunicationStatus.Disconnected);
        break;

      case "VS2":
        _systemState.SetCamera2Status(CommunicationStatus.Disconnected);
        break;

      case "VS3":
        _systemState.SetCamera3Status(CommunicationStatus.Disconnected);
        break;
    }
  }
}
