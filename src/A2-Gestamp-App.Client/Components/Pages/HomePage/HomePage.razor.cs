using Features.Inspection.Domain;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.HomePage;

public partial class HomePage : IDisposable
{
  [Inject]
  public IInspectionService InspectionService { get; set; } = default!;

  private int _currentIndex;

  private Inspection? CurrentInspection =>
      InspectionService.CurrentInspection;

  private CameraInspection? SelectedCamera =>
      CurrentInspection?.Cameras.ElementAtOrDefault(_currentIndex);

  protected override void OnInitialized()
  {
    InspectionService.InspectionChanged += OnInspectionChanged;
  }

  private void OnInspectionChanged(object? sender, Inspection inspection)
  {
    if (_currentIndex >= inspection.Cameras.Count)
    {
      _currentIndex = 0;
    }

    InvokeAsync(StateHasChanged);
  }

  private async Task ChangeCamera(int index)
  {
    if (CurrentInspection is null)
    {
      return;
    }

    if (index < 0 || index >= CurrentInspection.Cameras.Count)
    {
      return;
    }

    _currentIndex = index;

    await InvokeAsync(StateHasChanged);
  }

  public void Dispose()
  {
    InspectionService.InspectionChanged -= OnInspectionChanged;
  }
}
