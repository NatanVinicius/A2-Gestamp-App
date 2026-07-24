namespace A2GestampApp.Application.Features.Keyence;

public interface IKeyenceService
{
  public bool IsConnected { get; }

  public event EventHandler<bool>? ConnectionStatusChanged;

  public Task ConnectAsync();

  public Task DisconnectAsync();
}
