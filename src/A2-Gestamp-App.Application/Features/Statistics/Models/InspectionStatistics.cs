using InspectionModel = Features.Inspection.Domain.Inspection;

namespace Features.Statistics.Models;

public sealed class InspectionStatistics
{
  public int TotalInspections { get; internal set; }

  public int GoodInspections { get; internal set; }

  public int NoGoodInspections { get; internal set; }

  public double AverageExecutionTime { get; internal set; }

  public InspectionModel? LastInspection { get; internal set; }

  public double RejectRate =>
      TotalInspections == 0
          ? 0
          : (double)NoGoodInspections / TotalInspections * 100;
}
