namespace A2GestampApp.Client
{
  public partial class App : Microsoft.Maui.Controls.Application
  {
    public App()
    {
      InitializeComponent();
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

      return window;
    }
  }
}
