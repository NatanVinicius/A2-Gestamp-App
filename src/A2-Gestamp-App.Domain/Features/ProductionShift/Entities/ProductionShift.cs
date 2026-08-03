using A2GestampApp.Domain.Features.Inspection.Enums;
using A2GestampApp.Domain.Features.ProductionShift.Enums;

namespace A2GestampApp.Domain.Features.ProductionShift.Entities;

public sealed class ProductionShift
{
  public ProductionShiftNumber ShiftNumber { get; }

  public DateTime StartDate { get; }

  public DateTime EndDate { get; }

  public int Produced { get; private set; }

  public int Approved { get; private set; }

  public int Reproved { get; private set; }

  public InspectionResult LastInspectionResult { get; private set; }

  public TimeSpan LastCycleTime { get; private set; }

  public bool IsClosed { get; private set; }

  public DateTime CreatedAt { get; } = DateTime.Now;

  public double RejectionRate =>
      Produced == 0
          ? 0
          : (double)Reproved / Produced * 100;

  public ProductionShift(
      ProductionShiftNumber shiftNumber,
      DateTime startDate,
      DateTime endDate)
  {
    ShiftNumber = shiftNumber;
    StartDate = startDate;
    EndDate = endDate;
  }

  public void RegisterInspection(
    InspectionResult result,
    TimeSpan cycleTime)
  {
    Produced++;

    LastInspectionResult = result;
    LastCycleTime = cycleTime;

    if (result == InspectionResult.Aprovada)
    {
      Approved++;
    }
    else
    {
      Reproved++;
    }
  }

  public void ChangeJudgement(
    InspectionResult oldResult,
    InspectionResult newResult)
  {
    if (oldResult == newResult)
    {
      return;
    }

    if (oldResult == InspectionResult.Aprovada)
    {
      Approved--;
    }
    else
    {
      Reproved--;
    }

    if (newResult == InspectionResult.Aprovada)
    {
      Approved++;
    }
    else
    {
      Reproved++;
    }


    LastInspectionResult = newResult;
  }

  public void Close()
  {
    IsClosed = true;
  }
}
