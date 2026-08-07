namespace A2GestampApp.Infrastructure.Hikvision;

public interface IHikvisionUserService
{
  public Task<string> GenerateEmployeeIdAsync();

  public Task<byte[]> CaptureFaceAsync();

  public Task CreateUserAsync(
      string employeeId,
      string name,
      UserRole role);

  public Task CreateFaceRecordAsync(
      string employeeId,
      string name);

  public Task RegisterAsync(
      string employeeId,
      string name,
      UserRole role);
}
