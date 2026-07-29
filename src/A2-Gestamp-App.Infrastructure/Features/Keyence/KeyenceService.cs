using A2GestampApp.Application.Features.Keyence.Models;

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

  public KeyenceService(
    KeyenceOptions options,
    CameraMessageParser parser,
    ILogger<KeyenceService> logger,
    ILogger<KeyenceTcpConnection> connectionLogger)
  {
    _options = options;
    _logger = logger;
    _connectionLogger = connectionLogger;
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
}
