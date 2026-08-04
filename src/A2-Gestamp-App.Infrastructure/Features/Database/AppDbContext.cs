using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Entities;

using Microsoft.EntityFrameworkCore;

namespace A2GestampApp.Infrastructure.Features.Database;

public sealed class AppDbContext : DbContext
{
  public DbSet<Inspection> Inspections => Set<Inspection>();

  public DbSet<ProductionShift> ProductionShifts => Set<ProductionShift>();

  public AppDbContext(
      DbContextOptions<AppDbContext> options)
      : base(options)
  {
  }

  protected override void OnModelCreating(
      ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(AppDbContext).Assembly);

    base.OnModelCreating(modelBuilder);
  }
}
