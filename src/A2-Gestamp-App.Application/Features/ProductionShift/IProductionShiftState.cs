using A2GestampApp.Domain.Features.Inspection.Enums;
using A2GestampApp.Domain.Features.ProductionShift.Entities;

public interface IProductionShiftState
{
  public ProductionShift CurrentShift { get; }

  public event Action? StateChanged;

  public void RegisterInspection(
    InspectionResult result,
    TimeSpan cycleTime);

  public void NotifyStateChanged();
}
