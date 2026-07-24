public interface INetworkConnectionService
{
  public Task<bool> PingAsync(string ip);
}
