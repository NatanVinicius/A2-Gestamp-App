using A2GestampApp.Infrastructure.Features.Images;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddSingleton<INetworkConnectionService, NetworkConnectionService>();

    services.AddKeyence(configuration);
    services.AddImages();

    return services;
  }
}
