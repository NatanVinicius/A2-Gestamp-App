using A2GestampApp.Application.Features.Keyence;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Features.Keyence;

public sealed class FakeKeyenceService : IKeyenceService
{
  private readonly ILogger<FakeKeyenceService> _logger;

  public bool IsConnected { get; private set; }

  public event EventHandler<bool>? ConnectionStatusChanged;

  public FakeKeyenceService(ILogger<FakeKeyenceService> logger)
  {
    _logger = logger;
  }

  public Task ConnectAsync()
  {
    if (IsConnected)
    {
      _logger.LogInformation("Keyence is already connected.");
      return Task.CompletedTask;
    }

    try
    {
      _logger.LogInformation("Connecting to Keyence...");

      IsConnected = true;

      ConnectionStatusChanged?.Invoke(this, IsConnected);

      _logger.LogInformation("Keyence connected.");
    }
    catch (Exception ex)
    {
      IsConnected = false;

      _logger.LogError(ex, "Error while connecting to Keyence.");

      ConnectionStatusChanged?.Invoke(this, IsConnected);
    }

    return Task.CompletedTask;
  }

  public Task DisconnectAsync()
  {
    if (!IsConnected)
    {
      _logger.LogInformation("Keyence is already disconnected.");
      return Task.CompletedTask;
    }

    try
    {
      _logger.LogInformation("Disconnecting from Keyence...");

      IsConnected = false;

      ConnectionStatusChanged?.Invoke(this, IsConnected);

      _logger.LogInformation("Keyence disconnected.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error while disconnecting Keyence.");
    }

    return Task.CompletedTask;
  }
}
