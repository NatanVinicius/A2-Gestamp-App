using System.Net.NetworkInformation;

public class NetworkConnectionService : INetworkConnectionService
{
  public async Task<bool> PingAsync(string ip)
  {
    try
    {
      using var ping = new Ping();

      var reply = await ping.SendPingAsync(ip, 500);

      return reply.Status == IPStatus.Success;
    }
    catch
    {
      return false;
    }
  }
}
