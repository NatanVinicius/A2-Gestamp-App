using System.Net;

using A2GestampApp.Application.Features.Hikvision;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class FaceImageServer : IFaceImageServer, IDisposable
{
  private readonly HttpListener _listener = new();

  private CancellationTokenSource? _cts;

  private byte[]? _currentImage;

  public async Task StartAsync()
  {
    if (_listener.IsListening)
    {
      return;
    }

    _listener.Prefixes.Add("http://+:8080/");

    _listener.Start();

    _cts = new();

    _ = Task.Run(() => ListenAsync(_cts.Token));
  }

  public void SetImage(byte[] image)
  {
    _currentImage = image;
  }

  public void Clear()
  {
    _currentImage = null;
  }

  private async Task ListenAsync(
      CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      HttpListenerContext context;

      try
      {
        context = await _listener.GetContextAsync();
      }
      catch
      {
        break;
      }

      _ = Task.Run(() => HandleRequestAsync(context));
    }
  }

  private async Task HandleRequestAsync(
    HttpListenerContext context)
  {
    try
    {

      if (context.Request.Url?.AbsolutePath != "/face.jpg")
      {

        context.Response.StatusCode = 404;
        context.Response.Close();
        return;
      }

      if (_currentImage is null)
      {

        context.Response.StatusCode = 404;
        context.Response.Close();
        return;
      }

      context.Response.ContentType = "image/jpeg";
      context.Response.ContentLength64 = _currentImage.Length;

      await context.Response.OutputStream.WriteAsync(
          _currentImage,
          0,
          _currentImage.Length);

      await context.Response.OutputStream.FlushAsync();

      context.Response.Close();
    }
    catch (Exception)
    {

      try
      {
        context.Response.StatusCode = 500;
        context.Response.Close();
      }
      catch
      {
      }
    }
  }

  public void Dispose()
  {
    try
    {
      _cts?.Cancel();
    }
    catch
    {
    }

    if (_listener.IsListening)
    {
      _listener.Stop();
    }

    _listener.Close();
  }
}
