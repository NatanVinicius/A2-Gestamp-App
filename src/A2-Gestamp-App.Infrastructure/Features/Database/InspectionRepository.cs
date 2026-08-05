using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Enums;
using A2GestampApp.Infrastructure.Features.Database;

using Microsoft.EntityFrameworkCore;

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

  public async Task<List<Inspection>> GetAsync(
    DateOnly? date,
    ProductionShiftNumber? shift,
    CancellationToken cancellationToken = default)
  {
    var query = _context.Inspections
    .AsNoTracking()
    .AsQueryable();

    if (date is not null)
    {
      var selectedDate = date.Value.ToDateTime(TimeOnly.MinValue);

      query = query.Where(x =>
          x.Date.Date == selectedDate.Date);
    }

    if (shift is not null)
    {
      query = query.Where(x =>
          x.ProductionShiftId != null &&
          _context.ProductionShifts.Any(p =>
              p.Id == x.ProductionShiftId &&
              p.ShiftNumber == shift));
    }

    return await query
        .OrderByDescending(x => x.Date)
        .ToListAsync(cancellationToken);
  }
}
