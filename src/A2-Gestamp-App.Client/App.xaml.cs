using A2GestampApp.Application.Startup;

namespace A2GestampApp.Client
{
  public partial class App : Microsoft.Maui.Controls.Application
  {
    public App(IApplicationStartup startup)
    {
      InitializeComponent();


      _ = startup.StartAsync();

    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
      Window window = new(new MainPage())
      {
        Title = "A2 Gestamp App"
      };

#if WINDOWS
      window.Width = 1280;
      window.Height = 800;
#endif

      window.Destroying += async (_, _) =>
      {
        try
        {
          IApplicationStartup startup =
              Handler?.MauiContext?.Services.GetRequiredService<IApplicationStartup>()
              ?? throw new InvalidOperationException();

          await startup.StopAsync();
        }
        catch
        {
          // Ignora qualquer erro no encerramento.
        }
      };

      return window;

      return window;
    }
  }
}
