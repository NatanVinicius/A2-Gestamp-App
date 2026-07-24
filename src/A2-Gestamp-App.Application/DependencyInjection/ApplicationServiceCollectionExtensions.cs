using A2GestampApp.Application.Startup;

using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
  public static IServiceCollection AddApplication(
      this IServiceCollection services)
  {
    services.AddSingleton<IApplicationStartup, ApplicationStartup>();

    return services;
  }
}
