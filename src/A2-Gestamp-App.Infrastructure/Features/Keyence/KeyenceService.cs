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
    _logger.LogInformation(
        "[Keyence] Starting Keyence service. Cameras configured: {CameraCount}",
        _options.Cameras.Count);

    foreach (var camera in _options.Cameras)
    {
      _logger.LogInformation(
          "[Keyence] Creating connection for camera {Camera}. Host: {Host}, Port: {Port}",
          camera.Name,
          camera.Host,
          camera.Port);

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
        "[Keyence] Keyence service started.");
  }

  public async Task StopAsync()
  {
    _logger.LogInformation(
        "[Keyence] Stopping Keyence service. Connections: {ConnectionCount}",
        _connections.Count);

    foreach (var connection in _connections)
    {
      _logger.LogInformation(
          "[Keyence] Stopping connection for camera {Camera}.",
          connection.Camera.Name);

      connection.MessageReceived -= OnMessageReceived;
      connection.Connected -= OnConnected;
      connection.Disconnected -= OnDisconnected;

      await connection.DisconnectAsync();

      connection.Dispose();
    }

    _connections.Clear();

    _logger.LogInformation(
        "[Keyence] Keyence service stopped.");
  }

  private void OnMessageReceived(CameraMessage message)
  {
    _logger.LogInformation(
        "[Keyence] Message received from camera {Camera}. Raw message: {RawMessage}",
        message.CameraName,
        message.RawMessage);

    try
    {
      var inspection = _parser.Parse(message.RawMessage);

      _logger.LogInformation(
          "[Keyence] Message parsed successfully from camera {Camera}.",
          message.CameraName);

      _logger.LogInformation(
          "[Keyence] Inspection result received from camera {Camera}: {Result}",
          message.CameraName,
          inspection);

      InspectionReceived?.Invoke(inspection);

      _logger.LogInformation(
          "[Keyence] InspectionReceived event dispatched for camera {Camera}.",
          message.CameraName);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Keyence] Failed to parse message received from camera {Camera}. Raw message: {RawMessage}",
          message.CameraName,
          message.RawMessage);

      // A malformed packet must not terminate the camera receive loop.
      // The connection remains active and can process the next valid packet.
    }
  }

  public async Task SetModelAsync(int model)
  {
    string command = $"CWN,1,1,{model}\r";

    foreach (KeyenceTcpConnection connection in _connections)
    {
      try
      {
        await connection.SendAsync(command);
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "[Keyence] Failed to set model {Model} on camera {Camera}.",
            model,
            connection.Camera.Name);

        throw;
      }
    }
  }

  private void OnConnected(KeyenceTcpConnection connection)
  {
    _logger.LogInformation(
        "[Keyence] Camera {Camera} connected.",
        connection.Camera.Name);

    switch (connection.Camera.Name)
    {
      case "VS1":
        _systemState.SetCamera1Status(
            CommunicationStatus.Connected);
        break;

      case "VS2":
        _systemState.SetCamera2Status(
            CommunicationStatus.Connected);
        break;

      case "VS3":
        _systemState.SetCamera3Status(
            CommunicationStatus.Connected);
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
        _systemState.SetCamera1Status(
            CommunicationStatus.Disconnected);
        break;

      case "VS2":
        _systemState.SetCamera2Status(
            CommunicationStatus.Disconnected);
        break;

      case "VS3":
        _systemState.SetCamera3Status(
            CommunicationStatus.Disconnected);
        break;
    }
  }
}
