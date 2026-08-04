using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Infrastructure.Features.Database;

public sealed class InspectionRepository : IInspectionRepository
{
  private readonly AppDbContext _context;

  public InspectionRepository(
      AppDbContext context)
  {
    _context = context;
  }

  public async Task AddAsync(
      Inspection inspection,
      CancellationToken cancellationToken = default)
  {
    _context.Inspections.Add(inspection);

    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task UpdateAsync(
    Inspection inspection,
    CancellationToken cancellationToken = default)
  {
    _context.Inspections.Update(inspection);

    await _context.SaveChangesAsync(cancellationToken);
  }
}
