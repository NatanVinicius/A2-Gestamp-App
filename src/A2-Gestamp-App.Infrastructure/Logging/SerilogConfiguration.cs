using Serilog;

namespace A2_Gestamp_App.Infrastructure.Logging;

public static class SerilogConfiguration
{
  public static ILogger CreateLogger()
  {
    var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

    Directory.CreateDirectory(logDirectory);

    return new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.File(
            path: Path.Combine(logDirectory, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            shared: true)
        .CreateLogger();
  }
}
