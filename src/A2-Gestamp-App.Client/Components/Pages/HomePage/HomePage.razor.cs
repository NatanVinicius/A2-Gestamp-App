using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Domain.Features.Inspection.Models;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.HomePage;

public partial class HomePage : IDisposable
{
  [Inject]
  private IInspectionState InspectionState { get; set; } = default!;

  private Inspection? _inspection;

  private int _currentIndex;

  private CameraInspection? CurrentCamera =>
      _currentIndex switch
      {
        0 => _inspection?.Camera1,
        1 => _inspection?.Camera2,
        2 => _inspection?.Camera3,
        _ => null
      };

  protected override void OnInitialized()
  {
    _inspection = InspectionState.CurrentInspection;

    InspectionState.InspectionChanged += OnInspectionChanged;
  }

  private void OnInspectionChanged()
  {
    _inspection = InspectionState.CurrentInspection;

    InvokeAsync(StateHasChanged);
  }

  public void Dispose()
  {
    InspectionState.InspectionChanged -= OnInspectionChanged;
  }
}
