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
    var now = DateTime.Now;

    var today = now.Date;

    // Entre 01:09 e 05:59 pertence ao turno da manhã do dia atual.
    // Antes de 01:09 pertence ao turno da tarde do dia anterior.
    if (now.TimeOfDay < new TimeSpan(1, 9, 0))
    {
      today = today.AddDays(-1);
    }

    var morningStart = today.AddHours(6);

    var morningEnd = today
        .AddHours(15)
        .AddMinutes(48);

    var afternoonStart = morningEnd;

    var afternoonEnd = today
        .AddDays(1)
        .AddHours(1)
        .AddMinutes(9);

    if (now.TimeOfDay >= new TimeSpan(6, 0, 0) &&
        now.TimeOfDay < new TimeSpan(15, 48, 0))
    {
      return new ProductionShift(
          ProductionShiftNumber.Morning,
          morningStart,
          morningEnd);
    }

    if (now.TimeOfDay >= new TimeSpan(15, 48, 0))
    {
      return new ProductionShift(
          ProductionShiftNumber.Afternoon,
          afternoonStart,
          afternoonEnd);
    }

    // 01:09 até 05:59
    return new ProductionShift(
        ProductionShiftNumber.Morning,
        morningStart,
        morningEnd);
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
