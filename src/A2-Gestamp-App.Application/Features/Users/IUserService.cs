public interface IUserService
{
  public Task<User?> GetByEmployeeNumberAsync(
      string employeeNumber,
      CancellationToken cancellationToken = default);
}
