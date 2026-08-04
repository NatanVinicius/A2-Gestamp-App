using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Application.Features.Inspection.Services;
using A2GestampApp.Application.Features.Ng;
using A2GestampApp.Application.Features.System;
using A2GestampApp.Application.Startup;


using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
  public static IServiceCollection AddApplication(
      this IServiceCollection services)
  {
    services.AddSingleton<IApplicationStartup, ApplicationStartup>();

    services.AddSingleton<IInspectionCoordinator, InspectionCoordinator>();

    services.AddSingleton<IInspectionState, InspectionState>();

    services.AddSingleton<INgState, NgState>();

    services.AddSingleton<IAuthenticatedUserState, AuthenticatedUserState>();

    services.AddSingleton<IConfirmationDialogState, ConfirmationDialogState>();

    services.AddSingleton<IProductionShiftState, ProductionShiftState>();

    services.AddScoped<IInspectionReviewService, InspectionReviewService>();

    services.AddSingleton<ISystemState, SystemState>();

    return services;
  }
}
