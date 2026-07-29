using A2GestampApp.Infrastructure.Features.Keyence;

using Infrastructure.Features.Keyence.Parsers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
  public static IServiceCollection AddKeyence(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddSingleton(sp =>
    {
      IConfigurationSection section = configuration.GetSection("Keyence");

      return new KeyenceOptions
      {
        Cameras =
          [
              new CameraOptions
            {
                Name = section["Cameras:0:Name"] ?? string.Empty,
                Host = section["Cameras:0:Host"] ?? string.Empty,
                Port = int.Parse(section["Cameras:0:Port"]!)
            },
            new CameraOptions
            {
                Name = section["Cameras:1:Name"] ?? string.Empty,
                Host = section["Cameras:1:Host"] ?? string.Empty,
                Port = int.Parse(section["Cameras:1:Port"]!)
            },
            new CameraOptions
            {
                Name = section["Cameras:2:Name"] ?? string.Empty,
                Host = section["Cameras:2:Host"] ?? string.Empty,
                Port = int.Parse(section["Cameras:2:Port"]!)
            }
          ]
      };
    });

    services.AddSingleton<IKeyenceService, KeyenceService>();

    services.AddSingleton<CameraMessageParser>();

    return services;
  }
}
