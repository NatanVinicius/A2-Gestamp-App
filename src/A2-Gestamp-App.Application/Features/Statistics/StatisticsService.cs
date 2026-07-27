using Features.Inspection.Domain.Enums;
using Features.Statistics.Models;

using InspectionModel = Features.Inspection.Domain.Inspection;

namespace Features.Statistics;

public sealed class StatisticsService : IStatisticsService
{
  private readonly InspectionStatistics _statistics = new();

  public InspectionStatistics Current => _statistics;

  public event EventHandler<InspectionStatistics>? StatisticsChanged;

  public void AddInspection(InspectionModel inspection)
  {
    ArgumentNullException.ThrowIfNull(inspection);

    _statistics.TotalInspections++;

    _statistics.LastInspection = inspection;

    if (inspection.Judgement == InspectionJudgement.Aprovada)
    {
      _statistics.GoodInspections++;
    }
    else
    {
      _statistics.NoGoodInspections++;
    }

    _statistics.AverageExecutionTime = inspection.Cameras.Any()
     ? inspection.Cameras.Max(c => c.ExecutionTime)
     : 0;

    StatisticsChanged?.Invoke(this, _statistics);
  }

  public void Reset()
  {

    _statistics.TotalInspections = 0;
    _statistics.GoodInspections = 0;
    _statistics.NoGoodInspections = 0;
    _statistics.AverageExecutionTime = 0;
    _statistics.LastInspection = null;

    StatisticsChanged?.Invoke(this, _statistics);
  }
}
