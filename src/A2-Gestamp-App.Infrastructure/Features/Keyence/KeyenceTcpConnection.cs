using System.Net.Sockets;
using System.Text;

using A2GestampApp.Application.Features.Keyence.Models;

using Microsoft.Extensions.Logging;

namespace Infrastructure.Features.Keyence;

public sealed class KeyenceTcpConnection : IDisposable
{
  private readonly CameraOptions _camera;
  private readonly ILogger<KeyenceTcpConnection> _logger;

  private TcpClient? _client;
  private NetworkStream? _stream;

  private CancellationTokenSource? _cts;
  private Task? _receiveTask;

  public KeyenceTcpConnection(
      CameraOptions camera,
      ILogger<KeyenceTcpConnection> logger)
  {
    _camera = camera;
    _logger = logger;
  }

  public CameraOptions Camera => _camera;

  public bool IsConnected => _client?.Connected == true;

  public event Action<CameraMessage>? MessageReceived;

  public event Action<KeyenceTcpConnection>? Connected;
  public event Action<KeyenceTcpConnection>? Disconnected;

  public async Task ConnectAsync(
    CancellationToken cancellationToken = default)
  {
    if (IsConnected)
    {
      _logger.LogInformation(
          "[Keyence] Camera {Camera} is already connected.",
          _camera.Name);

      return;
    }

    _logger.LogInformation(
        "[Keyence] Connecting to camera {Camera}. Host: {Host}, Port: {Port}",
        _camera.Name,
        _camera.Host,
        _camera.Port);

    _client = new TcpClient();

    try
    {
      await _client.ConnectAsync(
        _camera.Host,
        _camera.Port,
        cancellationToken);

      _camera.isConnected = true;

      _logger.LogInformation(
          "[Keyence] Camera {Camera} connected successfully. Host: {Host}, Port: {Port}",
          _camera.Name,
          _camera.Host,
          _camera.Port);

      Connected?.Invoke(this);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Keyence] Failed to connect to camera {Camera}. Host: {Host}, Port: {Port}",
          _camera.Name,
          _camera.Host,
          _camera.Port);

      _camera.isConnected = false;

      Disconnected?.Invoke(this);

      throw;
    }

    _stream = _client.GetStream();

    _cts = new CancellationTokenSource();

    _receiveTask = ReceiveLoopAsync(_cts.Token);

    _logger.LogInformation(
        "[Keyence] Camera {Camera} receive loop started.",
        _camera.Name);

    _logger.LogInformation(
        "[Keyence] Camera {Camera} DataAvailable: {Available}",
        _camera.Name,
        _stream.DataAvailable);
  }

  private async Task ReceiveLoopAsync(
    CancellationToken cancellationToken)
  {
    if (_stream is null)
    {
      _logger.LogWarning(
          "[Keyence] Camera {Camera} receive loop started without an active stream.",
          _camera.Name);

      return;
    }

    var buffer = new byte[4096];

    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        var bytesRead = await _stream.ReadAsync(
            buffer,
            cancellationToken);

        if (bytesRead == 0)
        {
          _logger.LogWarning(
              "[Keyence] Camera {Camera} connection closed by remote device.",
              _camera.Name);

          Disconnected?.Invoke(this);

          break;
        }

        var message = Encoding.ASCII.GetString(
            buffer,
            0,
            bytesRead);

        _logger.LogInformation(
            "[Keyence] Data received from camera {Camera}. Bytes: {BytesRead}",
            _camera.Name,
            bytesRead);

        if (string.IsNullOrWhiteSpace(message))
        {
          _logger.LogInformation(
              "[Keyence] Empty message received from camera {Camera}.",
              _camera.Name);

          continue;
        }

        _logger.LogInformation(
            "[Keyence] Message received from camera {Camera}: {Message}",
            _camera.Name,
            message);

        MessageReceived?.Invoke(
            new CameraMessage(
                _camera.Name,
                message));
      }
    }
    catch (OperationCanceledException)
    {
      // Encerramento normal.

      _logger.LogInformation(
          "[Keyence] Receive loop cancelled for camera {Camera}.",
          _camera.Name);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Keyence] Communication error with camera {Camera}.",
          _camera.Name);
    }
  }

  public async Task DisconnectAsync()
  {
    _logger.LogInformation(
        "[Keyence] Disconnecting camera {Camera}.",
        _camera.Name);

    _cts?.Cancel();

    _stream?.Dispose();
    _client?.Dispose();

    if (_receiveTask is not null)
    {
      try
      {
        await _receiveTask;
      }
      catch
      {
        // Ignora exceções causadas pelo encerramento.
      }
    }

    _cts?.Dispose();

    _stream = null;
    _client = null;
    _cts = null;
    _receiveTask = null;

    _logger.LogInformation(
        "[Keyence] Camera {Camera} disconnected.",
        _camera.Name);
  }

  public void Dispose()
  {
    _logger.LogInformation(
        "[Keyence] Disposing connection for camera {Camera}.",
        _camera.Name);

    _cts?.Cancel();

    _stream?.Dispose();
    _client?.Dispose();
    _cts?.Dispose();
  }
}
