using A2GestampApp.Domain.Features.Inspection.Enums;
using A2GestampApp.Domain.Features.ProductionShift.Enums;

namespace A2GestampApp.Domain.Features.ProductionShift.Entities;

public sealed class ProductionShift
{
  public int Id { get; private set; }

  public ProductionShiftNumber ShiftNumber { get; }

  public DateTime StartDate { get; }

  public DateTime EndDate { get; }

  public int Produced { get; private set; }

  public int Approved { get; private set; }

  public int Reproved { get; private set; }

  public InspectionResult LastInspectionResult { get; private set; }

  public TimeSpan LastCycleTime { get; private set; }

  public bool IsClosed { get; private set; }

  public bool IsExpired =>
    DateTime.Now >= EndDate;

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

  public static ProductionShift CreateCurrent()
  {
    DateTime now = DateTime.Now;

    DateTime today = now.Date;

    if (now.TimeOfDay < new TimeSpan(6, 0, 0))
    {
      today = today.AddDays(-1);
    }

    if (now.TimeOfDay >= new TimeSpan(6, 0, 0) &&
        now.TimeOfDay < new TimeSpan(14, 0, 0))
    {
      return new ProductionShift(
          ProductionShiftNumber.Morning,
          today.AddHours(6),
          today.AddHours(14));
    }

    if (now.TimeOfDay >= new TimeSpan(14, 0, 0) &&
        now.TimeOfDay < new TimeSpan(22, 0, 0))
    {
      return new ProductionShift(
          ProductionShiftNumber.Afternoon,
          today.AddHours(14),
          today.AddHours(22));
    }

    return new ProductionShift(
        ProductionShiftNumber.Night,
        today.AddHours(22),
        today.AddDays(1).AddHours(6));
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
