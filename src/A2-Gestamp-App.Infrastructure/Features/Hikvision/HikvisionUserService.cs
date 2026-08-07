using System.Diagnostics;

using A2GestampApp.Infrastructure.Hikvision.Models.Requests;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class HikvisionUserService
    : IHikvisionUserService
{

  private readonly ILogger<FaceImageServer> _logger;

  private readonly HikvisionClient _client;

  private readonly Random _random = new();

  private readonly FaceImageServer _faceImageServer;

  public HikvisionUserService(
      HikvisionClient client,
      FaceImageServer faceImageServer,
      ILogger<FaceImageServer> logger)
  {
    _client = client;
    _faceImageServer = faceImageServer;
    _logger = logger;
  }


  public async Task<string> GenerateEmployeeIdAsync()
  {
    while (true)
    {
      var employeeId =
          _random.Next(1000, 9999)
                 .ToString();

      if (!await _client.UserExistsAsync(employeeId))
      {
        return employeeId;
      }
    }
  }
  public Task<byte[]> CaptureFaceAsync()
  {
    return _client.CaptureFaceAsync();
  }

  public async Task CreateUserAsync(
    string employeeId,
    string name,
    UserRole role)
  {
    var request =
        new CreateUserRequest
        {
          UserInfo = new UserInfo
          {
            EmployeeNo = employeeId,
            Name = name,
            UserType =
                    role == UserRole.AuxiliaryManager
                        ? "auxiliaryManager"
                        : "normal"
          }
        };

    await _client.CreateUserAsync(request);
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
    "FaceURL: {FaceUrl}",
    request.FaceUrl);

    await _client.CreateFaceRecordAsync(request);
  }

  public async Task RegisterAsync(
     string employeeId,
     string name,
     UserRole role)
  {

    var image = await CaptureFaceAsync();


    _faceImageServer.SetImage(image);


    try
    {
      await CreateUserAsync(
          employeeId,
          name,
          role);

      await CreateFaceRecordAsync(
          employeeId,
          name);
    }
    catch (Exception ex)
    {

      Debug.WriteLine(ex);
    }
    finally
    {
      _faceImageServer.Clear();
    }
  }




}
