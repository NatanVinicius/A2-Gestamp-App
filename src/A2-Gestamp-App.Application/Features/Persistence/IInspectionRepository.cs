using A2GestampApp.Domain.Features.Inspection.Entities;

public interface IInspectionRepository
{
  public Task AddAsync(
      Inspection inspection,
      CancellationToken cancellationToken = default);

  public Task UpdateAsync(
    Inspection inspection,
    CancellationToken cancellationToken = default);
}
