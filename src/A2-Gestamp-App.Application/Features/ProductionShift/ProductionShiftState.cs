using A2GestampApp.Domain.Features.Inspection.Enums;
using A2GestampApp.Domain.Features.ProductionShift.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Enums;

public sealed class ProductionShiftState
    : IProductionShiftState
{
  public ProductionShift CurrentShift { get; }

  public event Action? StateChanged;

  public ProductionShiftState()
  {
    CurrentShift = new ProductionShift(
        ProductionShiftNumber.Morning,
        DateTime.Today.AddHours(7),
        DateTime.Today.AddHours(15));
  }

  public void RegisterInspection(
    InspectionResult result,
    TimeSpan cycleTime)
  {
    CurrentShift.RegisterInspection(
        result,
        cycleTime);

    StateChanged?.Invoke();
  }

  public void NotifyStateChanged()
  {
    StateChanged?.Invoke();
  }
}
