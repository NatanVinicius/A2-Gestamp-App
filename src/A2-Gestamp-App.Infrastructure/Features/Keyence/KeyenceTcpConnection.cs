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
      return;
    }

    _client = new TcpClient();

    try
    {
      await _client.ConnectAsync(
        _camera.Host,
        _camera.Port,
        cancellationToken);

      _camera.isConnected = true;
      Connected?.Invoke(this);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to connect to camera {Camera}.",
          _camera.Name);

      _camera.isConnected = false;
      Disconnected?.Invoke(this);
      throw;
    }

    _stream = _client.GetStream();

    _cts = new CancellationTokenSource();

    _receiveTask = ReceiveLoopAsync(_cts.Token);

    _logger.LogInformation(
        "Camera {Camera} connected.",
        _camera.Name);

    _logger.LogInformation(
    "DataAvailable: {Available}",
    _stream.DataAvailable);
  }

  private async Task ReceiveLoopAsync(
    CancellationToken cancellationToken)
  {
    if (_stream is null)
    {
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
          Disconnected?.Invoke(this);
          break;
        }

        var message = Encoding.ASCII.GetString(
            buffer,
            0,
            bytesRead);

        if (string.IsNullOrWhiteSpace(message))
        {
          continue;
        }

        MessageReceived?.Invoke(
            new CameraMessage(
                _camera.Name,
                message));
      }
    }
    catch (OperationCanceledException)
    {
      // Encerramento normal.
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Communication error with camera {Camera}.",
          _camera.Name);
    }
  }
  public async Task DisconnectAsync()
  {
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
        "Camera {Camera} disconnected.",
        _camera.Name);
  }

  public void Dispose()
  {
    _cts?.Cancel();

    _stream?.Dispose();
    _client?.Dispose();
    _cts?.Dispose();
  }
}
