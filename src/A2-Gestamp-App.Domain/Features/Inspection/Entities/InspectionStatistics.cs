using A2GestampApp.Domain.Features.Inspection.Enums;

namespace A2GestampApp.Domain.Features.Inspection.Entities;

public sealed class InspectionStatistics
{
  public int Produced { get; private set; }

  public int Approved { get; private set; }

  public int Rejected { get; private set; }

  public InspectionResult LastInspectionResult { get; private set; }

  public TimeSpan LastCycleTime { get; private set; }

  public double RejectionRate =>
      Produced == 0
          ? 0
          : (double)Rejected / Produced * 100;

  public void Register(Inspection inspection)
  {
    Produced++;

    if (inspection.Approved)
    {
      Approved++;
    }
    else
    {
      Rejected++;
    }

    LastInspectionResult = inspection.Result;
    LastCycleTime = inspection.CycleTime;
  }
}
