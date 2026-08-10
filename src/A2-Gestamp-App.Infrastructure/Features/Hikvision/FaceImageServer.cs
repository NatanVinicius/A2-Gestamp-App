using System.Net;

using A2GestampApp.Application.Features.Hikvision;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class FaceImageServer
    : IFaceImageServer,
      IDisposable
{
  private readonly HttpListener _listener = new();
  private readonly ILogger<FaceImageServer> _logger;

  private CancellationTokenSource? _cts;

  private byte[]? _currentImage;

  public FaceImageServer(
      ILogger<FaceImageServer> logger)
  {
    _logger = logger;
  }

  public async Task StartAsync()
  {
    if (_listener.IsListening)
    {
      _logger.LogInformation(
          "[FaceImageServer] Server already listening.");

      return;
    }

    const string prefix = "http://+:8080/";

    _listener.Prefixes.Add(prefix);

    _logger.LogInformation(
        "[FaceImageServer] Starting HTTP listener on {Prefix}",
        prefix);

    try
    {
      _listener.Start();

      _logger.LogInformation(
          "[FaceImageServer] HTTP listener started successfully on port 8080.");
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceImageServer] Failed to start HTTP listener.");

      throw;
    }

    _cts = new CancellationTokenSource();

    _ = Task.Run(
        () => ListenAsync(_cts.Token));

    await Task.CompletedTask;
  }

  public void SetImage(byte[] image)
  {
    _currentImage = image;

    _logger.LogInformation(
        "[FaceImageServer] Image available. Size: {ImageSize} bytes.",
        image.Length);
  }

  public void Clear()
  {
    _currentImage = null;

    _logger.LogInformation(
        "[FaceImageServer] Current image cleared.");
  }

  private async Task ListenAsync(
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "[FaceImageServer] Listening for Hikvision image requests.");

    while (!cancellationToken.IsCancellationRequested)
    {
      HttpListenerContext context;

      try
      {
        context =
            await _listener.GetContextAsync();
      }
      catch (HttpListenerException)
          when (cancellationToken.IsCancellationRequested)
      {
        _logger.LogInformation(
            "[FaceImageServer] Listener stopped.");

        break;
      }
      catch (ObjectDisposedException)
          when (cancellationToken.IsCancellationRequested)
      {
        _logger.LogInformation(
            "[FaceImageServer] Listener disposed.");

        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "[FaceImageServer] Error while waiting for HTTP request.");

        break;
      }

      _logger.LogInformation(
          "[FaceImageServer] Request received. Method: {Method}, Path: {Path}, Remote: {Remote}",
          context.Request.HttpMethod,
          context.Request.Url?.AbsolutePath,
          context.Request.RemoteEndPoint);

      _ = Task.Run(
          () => HandleRequestAsync(context));
    }

    _logger.LogInformation(
        "[FaceImageServer] Listener loop finished.");
  }

  private async Task HandleRequestAsync(
      HttpListenerContext context)
  {
    try
    {
      var path =
          context.Request.Url?.AbsolutePath;

      if (path != "/face.jpg")
      {
        _logger.LogWarning(
            "[FaceImageServer] Invalid request path: {Path}",
            path);

        context.Response.StatusCode = 404;
        context.Response.Close();

        return;
      }

      if (_currentImage is null)
      {
        _logger.LogWarning(
            "[FaceImageServer] Image requested but no image is currently available.");

        context.Response.StatusCode = 404;
        context.Response.Close();

        return;
      }

      _logger.LogInformation(
          "[FaceImageServer] Sending image. Size: {ImageSize} bytes.",
          _currentImage.Length);

      context.Response.ContentType = "image/jpeg";

      context.Response.ContentLength64 =
          _currentImage.Length;

      await context.Response.OutputStream.WriteAsync(
          _currentImage,
          0,
          _currentImage.Length);

      await context.Response.OutputStream.FlushAsync();

      _logger.LogInformation(
          "[FaceImageServer] Image sent successfully.");

      context.Response.Close();
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceImageServer] Failed to serve face image.");

      try
      {
        context.Response.StatusCode = 500;
        context.Response.Close();
      }
      catch (Exception closeException)
      {
        _logger.LogError(
            closeException,
            "[FaceImageServer] Failed to close HTTP response after error.");
      }
    }
  }

  public void Dispose()
  {
    _logger.LogInformation(
        "[FaceImageServer] Disposing server.");

    try
    {
      _cts?.Cancel();
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[FaceImageServer] Error cancelling listener.");
    }

    if (_listener.IsListening)
    {
      _logger.LogInformation(
          "[FaceImageServer] Stopping HTTP listener.");

      _listener.Stop();
    }

    _listener.Close();

    _logger.LogInformation(
        "[FaceImageServer] Server disposed.");
  }
}
