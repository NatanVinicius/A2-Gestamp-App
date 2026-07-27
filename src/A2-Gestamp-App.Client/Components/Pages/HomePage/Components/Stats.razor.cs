using Features.Statistics;
using Features.Statistics.Models;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Pages.HomePage.Components;

public partial class Stats : ComponentBase, IDisposable
{
  [Inject]
  public IStatisticsService StatisticsService { get; set; } = default!;

  protected override void OnInitialized()
  {
    StatisticsService.StatisticsChanged += OnStatisticsChanged;
  }

  private void OnStatisticsChanged(
      object? sender,
      InspectionStatistics e)
  {
    InvokeAsync(StateHasChanged);
  }

  public void Dispose()
  {
    StatisticsService.StatisticsChanged -= OnStatisticsChanged;
  }
}
