public interface IPlcService
{
  public Task ConnectAsync();

  public Task DisconnectAsync();

  public Task WriteAsync(
      ushort register,
      ushort value);

  public Task<ushort> ReadAsync(
      ushort register);

  public Task SetWaitingAsync();

  public Task<int> ReadCurrentTableAsync();
}
