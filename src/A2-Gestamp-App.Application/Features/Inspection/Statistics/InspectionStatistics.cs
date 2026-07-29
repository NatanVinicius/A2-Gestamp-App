
using A2GestampApp.Domain.Features.Inspection.Entities;

namespace A2GestampApp.Application.Features.Inspection;

public sealed class InspectionStatisticsState : IInspectionStatisticsState
{
  public InspectionStatistics Statistics { get; private set; } = new();

  public event Action? StateChanged;

  public void SetStatistics(InspectionStatistics statistics)
  {
    Statistics = statistics;
    StateChanged?.Invoke();
  }
}
