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
      return new Window(new MainPage()) { Title = "A2GestampApp.Client" };
    }
  }
}
