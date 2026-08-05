using A2GestampApp.Domain.Features.ProductionShift.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Enums;
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

  public async Task<List<ProductionShift>> GetAsync(
    DateOnly? date,
    ProductionShiftNumber? shift,
    CancellationToken cancellationToken = default)
  {
    var query = _context.ProductionShifts
    .AsNoTracking()
    .AsQueryable();

    if (date is not null)
    {
      var selectedDate = date.Value.ToDateTime(TimeOnly.MinValue);

      query = query.Where(x =>
          x.StartDate.Date == selectedDate.Date);
    }

    if (shift is not null)
    {
      query = query.Where(x =>
          x.ShiftNumber == shift);
    }

    return await query
        .OrderByDescending(x => x.StartDate)
        .ToListAsync(cancellationToken);
  }
}
