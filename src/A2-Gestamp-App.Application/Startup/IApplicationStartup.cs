namespace A2GestampApp.Application.Startup;

public interface IApplicationStartup
{
  public Task StartAsync();

  public Task StopAsync();
}
