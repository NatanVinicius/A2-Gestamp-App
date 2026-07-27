using Features.Statistics.Models;

using InspectionModel = Features.Inspection.Domain.Inspection;

namespace Features.Statistics;

public interface IStatisticsService
{
  public InspectionStatistics Current { get; }

  public event EventHandler<InspectionStatistics>? StatisticsChanged;

  public void AddInspection(InspectionModel inspection);

  public void Reset();
}
