using System.Reflection;

using A2GestampApp.Application.DependencyInjection;
using A2GestampApp.Infrastructure.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    builder.Services.AddMauiBlazorWebView();

#if DEBUG
    builder.Services.AddBlazorWebViewDeveloperTools();
    builder.Logging.AddDebug();
#endif

    return builder.Build();
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
};
