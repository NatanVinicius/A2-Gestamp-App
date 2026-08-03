public sealed class User
{
  public required string EmployeeNumber { get; init; }

  public required string Name { get; init; }

  public required UserRole Role { get; init; }
}
