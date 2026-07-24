using A2GestampApp.Application.Features.Keyence;

using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Infrastructure.Features.Keyence;

public static class DependencyInjection
{
  public static IServiceCollection AddKeyence(
      this IServiceCollection services)
  {
    services.AddSingleton<IKeyenceCamera>(sp =>
        ActivatorUtilities.CreateInstance<FakeKeyenceCamera>(sp, 1));

    services.AddSingleton<IKeyenceCamera>(sp =>
        ActivatorUtilities.CreateInstance<FakeKeyenceCamera>(sp, 2));

    services.AddSingleton<IKeyenceCamera>(sp =>
        ActivatorUtilities.CreateInstance<FakeKeyenceCamera>(sp, 3));

    services.AddSingleton<KeyenceInspectionWiring>();

    return services;
  }
}
