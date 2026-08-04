using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace A2GestampApp.Infrastructure.Features.Database;

internal sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(
      string[] args)
  {
    var options =
        new DbContextOptionsBuilder<AppDbContext>();

    options.UseSqlite(
        "Data Source=gestamp.db");

    return new AppDbContext(options.Options);
  }
}
