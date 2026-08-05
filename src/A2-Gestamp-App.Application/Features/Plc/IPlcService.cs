public interface IPlcService
{
  public Task ConnectAsync();

  public Task DisconnectAsync();

  public Task WriteAsync(
      ushort register,
      ushort value);
}
