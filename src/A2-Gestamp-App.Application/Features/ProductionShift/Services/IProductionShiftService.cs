using A2GestampApp.Domain.Features.ProductionShift.Entities;

public interface IProductionShiftService
{
  public Task<ProductionShift> GetCurrentShiftAsync(
      CancellationToken cancellationToken = default);
}
