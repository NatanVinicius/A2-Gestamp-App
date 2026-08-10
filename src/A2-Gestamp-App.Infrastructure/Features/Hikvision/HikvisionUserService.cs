using A2GestampApp.Infrastructure.Hikvision.Models.Requests;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class HikvisionUserService
    : IHikvisionUserService
{
  private readonly ILogger<HikvisionUserService> _logger;

  private readonly HikvisionClient _client;

  private readonly Random _random = new();

  private readonly FaceImageServer _faceImageServer;

  public HikvisionUserService(
      HikvisionClient client,
      FaceImageServer faceImageServer,
      ILogger<HikvisionUserService> logger)
  {
    _client = client;
    _faceImageServer = faceImageServer;
    _logger = logger;
  }

  public async Task<string> GenerateEmployeeIdAsync()
  {
    _logger.LogInformation(
        "[HikvisionUserService] Generating employee ID.");

    while (true)
    {
      var employeeId =
          _random.Next(1000, 9999)
                 .ToString();

      if (!await _client.UserExistsAsync(employeeId))
      {
        _logger.LogInformation(
            "[HikvisionUserService] Employee ID generated: {EmployeeId}",
            employeeId);

        return employeeId;
      }

      _logger.LogInformation(
          "[HikvisionUserService] Generated employee ID already exists: {EmployeeId}",
          employeeId);
    }
  }

  public async Task<byte[]> CaptureFaceAsync()
  {
    _logger.LogInformation(
        "[HikvisionUserService] Starting face capture.");

    try
    {
      var image =
          await _client.CaptureFaceAsync();

      _logger.LogInformation(
          "[HikvisionUserService] Face captured successfully. Size: {ImageSize} bytes.",
          image.Length);

      return image;
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[HikvisionUserService] Failed to capture face.");

      throw;
    }
  }

  public async Task CreateUserAsync(
      string employeeId,
      string name,
      UserRole role)
  {
    var userType =
        role == UserRole.AuxiliaryManager
            ? "auxiliaryManager"
            : "normal";

    _logger.LogInformation(
        "[HikvisionUserService] Creating user. EmployeeId: {EmployeeId}, Name: {Name}, Role: {Role}, HikvisionUserType: {UserType}",
        employeeId,
        name,
        role,
        userType);

    var request =
        new CreateUserRequest
        {
          UserInfo = new UserInfo
          {
            EmployeeNo = employeeId,
            Name = name,
            UserType = userType
          }
        };

    try
    {
      await _client.CreateUserAsync(request);

      _logger.LogInformation(
          "[HikvisionUserService] User created successfully. EmployeeId: {EmployeeId}",
          employeeId);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[HikvisionUserService] Failed to create user. EmployeeId: {EmployeeId}, Name: {Name}",
          employeeId,
          name);

      throw;
    }
  }

  public async Task CreateFaceRecordAsync(
      string employeeId,
      string name)
  {
    var request =
        new CreateFaceRecordRequest
        {
          FaceUrl = "http://192.168.70.35:8080/face.jpg",
          Fpid = employeeId,
          Name = name
        };

    _logger.LogInformation(
        "[HikvisionUserService] Creating face record. EmployeeId: {EmployeeId}, Name: {Name}, FaceURL: {FaceUrl}",
        employeeId,
        name,
        request.FaceUrl);

    try
    {
      await _client.CreateFaceRecordAsync(request);

      _logger.LogInformation(
          "[HikvisionUserService] Face record created successfully. EmployeeId: {EmployeeId}",
          employeeId);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[HikvisionUserService] Failed to create face record. EmployeeId: {EmployeeId}, Name: {Name}",
          employeeId,
          name);

      throw;
    }
  }

  public async Task RegisterAsync(
      string employeeId,
      string name,
      UserRole role)
  {
    _logger.LogInformation(
        "[HikvisionUserService] Starting user registration. EmployeeId: {EmployeeId}, Name: {Name}, Role: {Role}",
        employeeId,
        name,
        role);

    var image =
        await CaptureFaceAsync();

    _faceImageServer.SetImage(image);

    _logger.LogInformation(
        "[HikvisionUserService] Face image made available to FaceImageServer. Size: {ImageSize} bytes.",
        image.Length);

    try
    {
      await CreateUserAsync(
          employeeId,
          name,
          role);

      await CreateFaceRecordAsync(
          employeeId,
          name);

      _logger.LogInformation(
          "[HikvisionUserService] User registration completed successfully. EmployeeId: {EmployeeId}",
          employeeId);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[HikvisionUserService] User registration failed. EmployeeId: {EmployeeId}, Name: {Name}, Role: {Role}",
          employeeId,
          name,
          role);

      throw;
    }
    finally
    {
      _faceImageServer.Clear();

      _logger.LogInformation(
          "[HikvisionUserService] Face image cleared after registration. EmployeeId: {EmployeeId}",
          employeeId);
    }
  }
}
