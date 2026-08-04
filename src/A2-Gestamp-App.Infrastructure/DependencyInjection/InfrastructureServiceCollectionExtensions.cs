using A2GestampApp.Infrastructure.Features.Database;
using A2GestampApp.Infrastructure.Features.Images;
using A2GestampApp.Infrastructure.Hikvision;

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

    services.AddSingleton<HikvisionClient>();
    services.AddSingleton<FaceRecognitionServer>();

    services.AddSingleton<IFaceRecognitionService, HikvisionFaceRecognitionService>();

    services.AddDatabase();

    return services;
  }
}
