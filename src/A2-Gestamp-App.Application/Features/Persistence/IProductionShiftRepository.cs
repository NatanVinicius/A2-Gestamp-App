using A2GestampApp.Domain.Features.ProductionShift.Entities;

public interface IProductionShiftRepository
{
  public Task<ProductionShift?> GetCurrentAsync(
      CancellationToken cancellationToken = default);

  public Task AddAsync(
      ProductionShift shift,
      CancellationToken cancellationToken = default);

  public Task UpdateAsync(
      ProductionShift shift,
      CancellationToken cancellationToken = default);
}
