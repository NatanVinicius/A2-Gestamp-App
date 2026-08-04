using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace A2GestampApp.Infrastructure.Features.Database;

public static class DatabaseDependencyInjection
{
  public static IServiceCollection AddDatabase(
      this IServiceCollection services)
  {
    var databasePath = Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "gestamp.db");

    Directory.CreateDirectory(
        Path.GetDirectoryName(databasePath)!);

    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(
            $"Data Source={databasePath}"));

    services.AddScoped<DatabaseInitializer>();

    services.AddScoped<IInspectionRepository, InspectionRepository>();

    services.AddScoped<IProductionShiftRepository, ProductionShiftRepository>();

    return services;
  }
}
