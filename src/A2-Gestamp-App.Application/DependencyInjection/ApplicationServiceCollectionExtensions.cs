using A2GestampApp.Application.Startup;


using Features.Inspection.Domain;
using Features.Statistics;

using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
  public static IServiceCollection AddApplication(
      this IServiceCollection services)
  {
    services.AddSingleton<IApplicationStartup, ApplicationStartup>();

    services.AddSingleton<IInspectionService, InspectionService>();

    services.AddSingleton<IStatisticsService, StatisticsService>();

    services.AddSingleton<StatisticsWiring>();

    return services;
  }
}
