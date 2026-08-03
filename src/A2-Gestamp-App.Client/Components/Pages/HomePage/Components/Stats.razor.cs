using A2GestampApp.Domain.Features.ProductionShift.Entities;

using Microsoft.AspNetCore.Components;


namespace A2GestampApp.Client.Components.Pages.HomePage.Components;

public partial class Stats : ComponentBase, IDisposable
{
  [Inject]
  private IProductionShiftState ProductionShiftState { get; set; } = default!;

  private ProductionShift Statistics => ProductionShiftState.CurrentShift;

  protected override void OnInitialized()
  {
    ProductionShiftState.StateChanged += OnStateChanged;
  }

  private void OnStateChanged()
  {
    InvokeAsync(StateHasChanged);
  }

  public void Dispose()
  {
    ProductionShiftState.StateChanged -= OnStateChanged;
  }
}
