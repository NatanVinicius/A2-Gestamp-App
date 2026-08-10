using System.Net;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class FaceRecognitionServer : IDisposable
{
  private readonly HttpListener _listener = new();
  private readonly ILogger<FaceRecognitionServer> _logger;

  private CancellationTokenSource? _cts;

  public event Func<HttpListenerRequest, Task>? RequestReceived;

  public event Action? Started;

  public event Action? Stopped;

  public FaceRecognitionServer(
      ILogger<FaceRecognitionServer> logger)
  {
    _logger = logger;
  }

  public async Task StartAsync(int port)
  {
    if (_listener.IsListening)
    {
      _logger.LogInformation(
          "[FaceRecognitionServer] Server already listening.");

      return;
    }

    var prefix = $"http://+:{port}/";

    _listener.Prefixes.Add(prefix);

    _logger.LogInformation(
        "[FaceRecognitionServer] Starting HTTP listener on {Prefix}",
        prefix);

    try
    {
      _listener.Start();

      _logger.LogInformation(
          "[FaceRecognitionServer] HTTP listener started successfully on port {Port}.",
          port);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceRecognitionServer] Failed to start HTTP listener on port {Port}.",
          port);

      throw;
    }

    Started?.Invoke();

    _cts = new CancellationTokenSource();

    _ = ListenAsync(_cts.Token);

    await Task.CompletedTask;
  }

  private async Task ListenAsync(
      CancellationToken token)
  {
    _logger.LogInformation(
        "[FaceRecognitionServer] Listening for Hikvision events.");

    while (!token.IsCancellationRequested)
    {
      HttpListenerContext context;

      try
      {
        context =
            await _listener.GetContextAsync();
      }
      catch (HttpListenerException)
          when (token.IsCancellationRequested)
      {
        _logger.LogInformation(
            "[FaceRecognitionServer] Listener stopped.");

        break;
      }
      catch (ObjectDisposedException)
          when (token.IsCancellationRequested)
      {
        _logger.LogInformation(
            "[FaceRecognitionServer] Listener disposed.");

        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "[FaceRecognitionServer] Error while waiting for HTTP request.");

        Stopped?.Invoke();

        throw;
      }

      _logger.LogInformation(
          "[FaceRecognitionServer] Request received. Method: {Method}, Path: {Path}, Remote: {Remote}",
          context.Request.HttpMethod,
          context.Request.Url?.AbsolutePath,
          context.Request.RemoteEndPoint);

      try
      {
        if (RequestReceived is not null)
        {
          await RequestReceived(context.Request);
        }

        context.Response.StatusCode = 200;

        _logger.LogInformation(
            "[FaceRecognitionServer] Response sent. Status: {StatusCode}",
            context.Response.StatusCode);
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "[FaceRecognitionServer] Error processing HTTP request.");

        context.Response.StatusCode = 500;
      }
      finally
      {
        try
        {
          context.Response.Close();
        }
        catch (Exception ex)
        {
          _logger.LogError(
              ex,
              "[FaceRecognitionServer] Error closing HTTP response.");
        }
      }
    }

    _logger.LogInformation(
        "[FaceRecognitionServer] Listener loop finished.");
  }

  public void Dispose()
  {
    _logger.LogInformation(
        "[FaceRecognitionServer] Disposing server.");

    try
    {
      _cts?.Cancel();
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceRecognitionServer] Error cancelling listener.");
    }

    if (_listener.IsListening)
    {
      Stopped?.Invoke();

      _logger.LogInformation(
          "[FaceRecognitionServer] Stopping HTTP listener.");

      _listener.Stop();
    }

    _listener.Close();

    _logger.LogInformation(
        "[FaceRecognitionServer] Server disposed.");
  }
}
