using Features.Inspection.Domain;

namespace Features.Statistics;

public sealed class StatisticsWiring
{
  public StatisticsWiring(
      IInspectionService inspectionService,
      IStatisticsService statisticsService)
  {
    inspectionService.InspectionCompleted += (_, inspection) =>
    {
      if (inspection is not null)
      {
        statisticsService.AddInspection(inspection);
      }
    };
  }
}
