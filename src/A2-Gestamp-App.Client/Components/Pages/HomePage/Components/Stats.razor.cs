using A2GestampApp.Application.Features.Inspection;
using A2GestampApp.Domain.Features.Inspection.Entities;

using Microsoft.AspNetCore.Components;


namespace A2GestampApp.Client.Components.Pages.HomePage.Components;

public partial class Stats : ComponentBase, IDisposable
{
  [Inject]
  private IInspectionStatisticsState InspectionStatisticsState { get; set; } = default!;

  private InspectionStatistics Statistics => InspectionStatisticsState.Statistics;

  protected override void OnInitialized()
  {
    InspectionStatisticsState.StateChanged += OnStateChanged;
  }

  private void OnStateChanged()
  {
    InvokeAsync(StateHasChanged);
  }

  public void Dispose()
  {
    InspectionStatisticsState.StateChanged -= OnStateChanged;
  }
}
