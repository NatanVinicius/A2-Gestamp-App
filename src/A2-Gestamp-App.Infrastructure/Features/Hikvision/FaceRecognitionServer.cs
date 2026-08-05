using System.Diagnostics;
using System.Net;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class FaceRecognitionServer : IDisposable
{
  private readonly HttpListener _listener = new();

  private CancellationTokenSource? _cts;

  public event Func<HttpListenerRequest, Task>? RequestReceived;

  public event Action? Started;

  public event Action? Stopped;

  public async Task StartAsync(int port)
  {
    if (_listener.IsListening)
    {
      return;
    }

    _listener.Prefixes.Add($"http://+:{port}/");

    try
    {
      _listener.Start();

      Debug.WriteLine("HttpListener iniciado.");
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.ToString());
      throw;
    }

    Started?.Invoke();

    Debug.WriteLine("Listener started");

    _cts = new CancellationTokenSource();

    _ = ListenAsync(_cts.Token);

    await Task.CompletedTask;
  }

  private async Task ListenAsync(CancellationToken token)
  {
    Debug.WriteLine("ListenAsync started");

    while (!token.IsCancellationRequested)
    {
      Debug.WriteLine("Waiting request...");

      HttpListenerContext context;

      try
      {
        context = await _listener.GetContextAsync();
      }
      catch (Exception ex)
      {
        Stopped?.Invoke();
        Debug.WriteLine(ex);
        throw;
      }

      if (RequestReceived is not null)
      {
        await RequestReceived(context.Request);
      }

      context.Response.StatusCode = 200;
      context.Response.Close();
    }
  }


  public void Dispose()
  {
    _cts?.Cancel();

    if (_listener.IsListening)
    {
      Stopped?.Invoke();
      _listener.Stop();
    }

    _listener.Close();
  }
}
