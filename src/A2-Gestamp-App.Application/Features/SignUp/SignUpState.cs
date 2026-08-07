public sealed class SignUpState : ISignUpState
{
  private readonly Random _random = new();

  public string Name { get; set; } = string.Empty;

  public string EmployeeId { get; private set; } = string.Empty;

  public UserRole Role { get; set; } = UserRole.Normal;

  public void GenerateEmployeeId()
  {
    EmployeeId = _random
        .Next(1000, 9999)
        .ToString();
  }

  public void Reset()
  {
    Name = string.Empty;

    Role = UserRole.Normal;

    GenerateEmployeeId();
  }
}
