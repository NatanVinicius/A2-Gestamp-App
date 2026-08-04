using A2GestampApp.Infrastructure.Features.Database;

using Microsoft.EntityFrameworkCore;

public sealed class DatabaseInitializer
{
  private readonly AppDbContext _context;

  public DatabaseInitializer(
      AppDbContext context)
  {
    _context = context;
  }

  public void Initialize()
  {
    _context.Database.Migrate();
  }
}
