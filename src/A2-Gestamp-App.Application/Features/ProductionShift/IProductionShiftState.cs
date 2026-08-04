using A2GestampApp.Domain.Features.ProductionShift.Entities;

public interface IProductionShiftState
{
  public ProductionShift CurrentShift { get; }

  public event Action? StateChanged;

  public void SetCurrentShift(
      ProductionShift shift);

  public void NotifyStateChanged();
}
