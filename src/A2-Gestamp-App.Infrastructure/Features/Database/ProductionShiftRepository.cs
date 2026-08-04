using A2GestampApp.Domain.Features.ProductionShift.Entities;
using A2GestampApp.Infrastructure.Features.Database;

using Microsoft.EntityFrameworkCore;

public sealed class ProductionShiftRepository
    : IProductionShiftRepository
{
  private readonly AppDbContext _context;

  public ProductionShiftRepository(
      AppDbContext context)
  {
    _context = context;
  }

  public async Task<ProductionShift?> GetCurrentAsync(
      CancellationToken cancellationToken = default)
  {
    return await _context.ProductionShifts
        .SingleOrDefaultAsync(x => !x.IsClosed, cancellationToken);
  }

  public async Task AddAsync(
      ProductionShift shift,
      CancellationToken cancellationToken = default)
  {
    _context.ProductionShifts.Add(shift);

    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task UpdateAsync(
      ProductionShift shift,
      CancellationToken cancellationToken = default)
  {
    _context.ProductionShifts.Update(shift);

    await _context.SaveChangesAsync(cancellationToken);
  }
}
