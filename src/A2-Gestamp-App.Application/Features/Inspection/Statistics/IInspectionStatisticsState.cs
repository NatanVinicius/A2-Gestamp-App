using A2GestampApp.Domain.Features.Inspection.Entities;

namespace A2GestampApp.Application.Features.Inspection;

public interface IInspectionStatisticsState
{
  public InspectionStatistics Statistics { get; }

  public event Action? StateChanged;

  public void SetStatistics(InspectionStatistics statistics);
}
