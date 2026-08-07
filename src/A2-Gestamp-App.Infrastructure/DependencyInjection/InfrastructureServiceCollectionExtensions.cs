using A2GestampApp.Application.Features.FaceCapture;
using A2GestampApp.Application.Features.Hikvision;
using A2GestampApp.Infrastructure.Features.Database;
using A2GestampApp.Infrastructure.Features.Images;
using A2GestampApp.Infrastructure.Features.Plc;
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

    services.AddSingleton<IPlcService, PlcService>();

    services.AddSingleton<IHikvisionUserService, HikvisionUserService>();

    services.AddSingleton<FaceImageServer>();

    services.AddSingleton<IFaceImageServer>(provider =>
        provider.GetRequiredService<FaceImageServer>());

    services.AddSingleton<IFaceCaptureState, FaceCaptureState>();

    return services;
  }
}
