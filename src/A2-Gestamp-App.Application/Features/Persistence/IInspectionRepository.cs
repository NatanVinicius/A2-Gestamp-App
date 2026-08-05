using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Enums;

public interface IInspectionRepository
{
  public Task AddAsync(
      Inspection inspection,
      CancellationToken cancellationToken = default);

  public Task UpdateAsync(
      Inspection inspection,
      CancellationToken cancellationToken = default);

  public Task<List<Inspection>> GetAsync(
    DateOnly? date,
    ProductionShiftNumber? shift,
    CancellationToken cancellationToken = default);
}
