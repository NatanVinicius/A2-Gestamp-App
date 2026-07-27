using System.Reflection;

using A2_Gestamp_App.Infrastructure.Logging;

using A2GestampApp.Application.DependencyInjection;
using A2GestampApp.Infrastructure.DependencyInjection;
using A2GestampApp.Infrastructure.Features.Keyence;

using Features.Statistics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;

namespace A2GestampApp.Client;

public static class MauiProgram
{
  public static MauiApp CreateMauiApp()
  {
    var builder = MauiApp.CreateBuilder();

    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts =>
        {
          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });

    ConfigureConfiguration(builder);

    Log.Logger = SerilogConfiguration.CreateLogger();

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger);

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    builder.Services.Configure<NetworkSettings>(
        builder.Configuration.GetSection("Network"));

    builder.Services.AddMauiBlazorWebView();

#if DEBUG
    builder.Services.AddBlazorWebViewDeveloperTools();
    builder.Logging.AddDebug();
#endif

    MauiApp app = builder.Build();

    app.Services.GetRequiredService<KeyenceInspectionWiring>();
    app.Services.GetRequiredService<StatisticsWiring>();

    return app;
  }

  private static void ConfigureConfiguration(MauiAppBuilder builder)
  {
    Assembly assembly = Assembly.GetExecutingAssembly();

    string resourceName = assembly
        .GetManifestResourceNames()
        .Single(name => name.EndsWith("appsettings.json"));

    using Stream stream = assembly.GetManifestResourceStream(resourceName)!;

    IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonStream(stream)
        .Build();

    builder.Configuration.AddConfiguration(configuration);
  }
}
