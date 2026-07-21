using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    return services;
  }
}
