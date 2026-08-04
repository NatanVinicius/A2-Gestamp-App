using A2GestampApp.Domain.Features.ProductionShift.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Enums;

public sealed class ProductionShiftState
    : IProductionShiftState
{
  public ProductionShift CurrentShift { get; private set; }

  public event Action? StateChanged;

  public ProductionShiftState()
  {
    CurrentShift = new ProductionShift(
        ProductionShiftNumber.Morning,
        DateTime.Today.AddHours(7),
        DateTime.Today.AddHours(15));
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
