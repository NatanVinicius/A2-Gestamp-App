using A2GestampApp.Domain.Features.ProductionShift.Entities;

public sealed class ProductionShiftState
    : IProductionShiftState
{
  public ProductionShift CurrentShift { get; private set; }

  public event Action? StateChanged;

  public ProductionShiftState()
  {
    CurrentShift = ProductionShift.CreateCurrent();
  }

  public void NotifyStateChanged()
  {
    StateChanged?.Invoke();
  }

  public void SetCurrentShift(
    ProductionShift shift)
  {
    CurrentShift = shift;

    StateChanged?.Invoke();
  }
}
