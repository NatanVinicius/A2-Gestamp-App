public interface ISignUpState
{
  public string Name { get; set; }

  public string EmployeeId { get; }

  public UserRole Role { get; set; }

  public void GenerateEmployeeId();

  public void Reset();
}
