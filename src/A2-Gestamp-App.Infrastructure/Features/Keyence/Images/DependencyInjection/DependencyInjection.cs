using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Infrastructure.Features.Images.Services;

using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Infrastructure.Features.Images;

public static class DependencyInjection
{
  public static IServiceCollection AddImages(
    this IServiceCollection services)
  {
    services.AddSingleton<IImageWatcherService, ImageWatcherService>();

    services.AddSingleton<IImageTransferService, ImageTransferService>();

    return services;
  }
}
