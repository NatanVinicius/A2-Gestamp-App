using System.Net;
using System.Text;
using System.Text.Json;

using A2GestampApp.Infrastructure.Hikvision.Models.Requests;
using A2GestampApp.Infrastructure.Hikvision.Models.Responses;

using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class HikvisionClient
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<HikvisionClient> _logger;

  public HikvisionClient(
      ILogger<HikvisionClient> logger)
  {
    _logger = logger;

    var handler = new HttpClientHandler
    {
      Credentials = new NetworkCredential(
          "admin",
          "@2Vision")
    };

    _httpClient = new HttpClient(handler);
  }

  public async Task RegisterHttpHostAsync(
      string deviceAddress,
      string platformIp,
      int platformPort)
  {
    var url =
        $"http://{deviceAddress}/ISAPI/Event/notification/httpHosts/1";

    var xml =
$"""
<?xml version="1.0" encoding="UTF-8"?>
<HttpHostNotification version="2.0" xmlns="http://www.isapi.org/ver20/XMLSchema">
    <id>1</id>
    <url>/</url>
    <protocolType>HTTP</protocolType>
    <parameterFormatType>XML</parameterFormatType>
    <addressingFormatType>ipaddress</addressingFormatType>
    <ipAddress>{platformIp}</ipAddress>
    <portNo>{platformPort}</portNo>
    <httpAuthenticationMethod>none</httpAuthenticationMethod>
</HttpHostNotification>
""";

    _logger.LogInformation(
        "[Hikvision] Registering HTTP host. Device: {DeviceAddress}, Platform: {PlatformIp}:{PlatformPort}",
        deviceAddress,
        platformIp,
        platformPort);

    using var request = new HttpRequestMessage(
        HttpMethod.Put,
        url);

    request.Content = new StringContent(
        xml,
        Encoding.UTF8,
        "application/xml");

    try
    {
      using var response =
          await _httpClient.SendAsync(request);

      var body =
          await response.Content.ReadAsStringAsync();

      _logger.LogInformation(
          "[Hikvision] Register HTTP host response. Status: {StatusCode}, Response: {Response}",
          (int)response.StatusCode,
          body);

      response.EnsureSuccessStatusCode();
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Hikvision] Failed to register HTTP host. Device: {DeviceAddress}, Platform: {PlatformIp}:{PlatformPort}",
          deviceAddress,
          platformIp,
          platformPort);

      throw;
    }
  }

  public async Task<byte[]> CaptureFaceAsync()
  {
    const string url =
        "http://192.168.70.40/ISAPI/AccessControl/CaptureFaceData";

    const string xml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<CaptureFaceDataCond xmlns="http://www.isapi.org/ver20/XMLSchema" version="2.0">
    <captureInfrared>true</captureInfrared>
    <dataType>binary</dataType>
    <readerID>1</readerID>
</CaptureFaceDataCond>
""";

    _logger.LogInformation(
        "[Hikvision] Capturing face image.");

    using var request = new HttpRequestMessage(
        HttpMethod.Post,
        url);

    request.Content = new StringContent(
        xml,
        Encoding.UTF8,
        "application/xml");

    try
    {
      using var response = await _httpClient.SendAsync(
          request,
          HttpCompletionOption.ResponseHeadersRead);

      _logger.LogInformation(
          "[Hikvision] Capture face response. Status: {StatusCode}",
          (int)response.StatusCode);

      response.EnsureSuccessStatusCode();

      await using var stream =
          await response.Content.ReadAsStreamAsync();

      var image = await HikvisionJpegExtractor
          .ExtractAsync(stream);

      _logger.LogInformation(
          "[Hikvision] Face image captured successfully. Size: {ImageSize} bytes.",
          image.Length);

      return image;
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Hikvision] Failed to capture face image.");

      throw;
    }
  }

  public async Task<User?> SearchUserAsync(
      string deviceAddress,
      string employeeNumber)
  {
    var url =
        $"http://{deviceAddress}/ISAPI/AccessControl/UserInfo/Search?format=json";

    _logger.LogInformation(
        "[Hikvision] Searching user. EmployeeNumber: {EmployeeNumber}",
        employeeNumber);

    var body =
$$"""
{
  "UserInfoSearchCond": {
    "searchID": "1",
    "searchResultPosition": 0,
    "maxResults": 1,
    "EmployeeNoList": [
      {
        "employeeNo": "{{employeeNumber}}"
      }
    ]
  }
}
""";

    using var request = new HttpRequestMessage(
        HttpMethod.Post,
        url);

    request.Content = new StringContent(
        body,
        Encoding.UTF8,
        "application/json");

    try
    {
      using var response =
          await _httpClient.SendAsync(request);

      var json =
          await response.Content.ReadAsStringAsync();

      _logger.LogInformation(
          "[Hikvision] Search user response. EmployeeNumber: {EmployeeNumber}, Status: {StatusCode}",
          employeeNumber,
          (int)response.StatusCode);

      response.EnsureSuccessStatusCode();

      using var document =
          JsonDocument.Parse(json);

      if (!document.RootElement.TryGetProperty(
              "UserInfoSearch",
              out var search))
      {
        _logger.LogInformation(
            "[Hikvision] User not found. EmployeeNumber: {EmployeeNumber}",
            employeeNumber);

        return null;
      }

      if (!search.TryGetProperty(
              "UserInfo",
              out var users))
      {
        _logger.LogInformation(
            "[Hikvision] User not found. EmployeeNumber: {EmployeeNumber}",
            employeeNumber);

        return null;
      }

      if (users.ValueKind != JsonValueKind.Array ||
          users.GetArrayLength() == 0)
      {
        _logger.LogInformation(
            "[Hikvision] User not found. EmployeeNumber: {EmployeeNumber}",
            employeeNumber);

        return null;
      }

      var user = users[0];

      var name =
          user.TryGetProperty(
              "name",
              out var nameProperty)
              ? nameProperty.GetString() ?? string.Empty
              : string.Empty;

      var userType =
          user.TryGetProperty(
              "userType",
              out var typeProperty)
              ? typeProperty.GetString() ?? string.Empty
              : string.Empty;

      var result = new User
      {
        EmployeeNumber = employeeNumber,
        Name = name,
        Role =
            userType.Equals(
                "auxiliaryManager",
                StringComparison.OrdinalIgnoreCase)
                ? UserRole.AuxiliaryManager
                : UserRole.Normal
      };

      _logger.LogInformation(
          "[Hikvision] User found. EmployeeNumber: {EmployeeNumber}, Name: {Name}, Role: {Role}",
          result.EmployeeNumber,
          result.Name,
          result.Role);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Hikvision] Failed to search user. EmployeeNumber: {EmployeeNumber}",
          employeeNumber);

      throw;
    }
  }

  public async Task SetCardReaderEnabledAsync(bool enabled)
  {
    const string url =
        "http://192.168.70.40/ISAPI/AccessControl/CardReaderCfg/1?format=json";

    _logger.LogInformation(
        "[Hikvision] Setting card reader. Enabled: {Enabled}",
        enabled);

    using var request = new HttpRequestMessage(
        HttpMethod.Put,
        url);

    var json = $@"
      {{
        ""CardReaderCfg"": {{
          ""enable"": {enabled.ToString().ToLowerInvariant()}
        }}
      }}";

    request.Content = new StringContent(
        json,
        Encoding.UTF8,
        "application/json");

    try
    {
      using var response =
          await _httpClient.SendAsync(request);

      var responseBody =
          await response.Content.ReadAsStringAsync();

      _logger.LogInformation(
          "[Hikvision] Card reader response. Enabled: {Enabled}, Status: {StatusCode}, Response: {Response}",
          enabled,
          (int)response.StatusCode,
          responseBody);

      response.EnsureSuccessStatusCode();
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Hikvision] Failed to set card reader. Enabled: {Enabled}",
          enabled);

      throw;
    }
  }

  public async Task<bool> UserExistsAsync(
      string employeeNumber)
  {
    _logger.LogInformation(
        "[Hikvision] Checking if user exists. EmployeeNumber: {EmployeeNumber}",
        employeeNumber);

    var user = await SearchUserAsync(
        "192.168.70.40",
        employeeNumber);

    var exists = user is not null;

    _logger.LogInformation(
        "[Hikvision] User existence check. EmployeeNumber: {EmployeeNumber}, Exists: {Exists}",
        employeeNumber,
        exists);

    return exists;
  }

  public async Task CreateUserAsync(
      CreateUserRequest request)
  {
    const string url =
        "http://192.168.70.40/ISAPI/AccessControl/UserInfo/Record?format=json";

    _logger.LogInformation(
        "[Hikvision] Creating user. EmployeeNumber: {EmployeeNumber}, Name: {Name}, Role: {UserType}",
        request.UserInfo.EmployeeNo,
        request.UserInfo.Name,
        request.UserInfo.UserType);

    var serializedRequest =
        JsonSerializer.Serialize(request);

    using var httpRequest = new HttpRequestMessage(
        HttpMethod.Post,
        url);

    httpRequest.Content = new StringContent(
        serializedRequest,
        Encoding.UTF8,
        "application/json");

    try
    {
      using var response =
          await _httpClient.SendAsync(httpRequest);

      var json =
          await response.Content.ReadAsStringAsync();

      _logger.LogInformation(
          "[Hikvision] Create user response. EmployeeNumber: {EmployeeNumber}, Status: {StatusCode}, Response: {Response}",
          request.UserInfo.EmployeeNo,
          (int)response.StatusCode,
          json);

      response.EnsureSuccessStatusCode();

      var result =
          JsonSerializer.Deserialize<CreateUserResponse>(json);

      if (result is null)
      {
        throw new Exception(
            "Resposta inválida da Hikvision.");
      }

      if (!result.Success)
      {
        throw new Exception(
            result.ErrorMessage ??
            result.SubStatusCode ??
            result.StatusString);
      }

      _logger.LogInformation(
          "[Hikvision] User created successfully. EmployeeNumber: {EmployeeNumber}",
          request.UserInfo.EmployeeNo);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Hikvision] Failed to create user. EmployeeNumber: {EmployeeNumber}",
          request.UserInfo.EmployeeNo);

      throw;
    }
  }

  public async Task CreateFaceRecordAsync(
      CreateFaceRecordRequest request)
  {
    const string url =
        "http://192.168.70.40/ISAPI/Intelligent/FDLib/FaceDataRecord?format=json";

    _logger.LogInformation(
        "[Hikvision] Creating face record. FPID: {Fpid}, Name: {Name}, FaceURL: {FaceUrl}",
        request.Fpid,
        request.Name,
        request.FaceUrl);

    var serializedRequest =
        JsonSerializer.Serialize(request);

    using var httpRequest = new HttpRequestMessage(
        HttpMethod.Post,
        url);

    httpRequest.Content = new StringContent(
        serializedRequest,
        Encoding.UTF8,
        "application/json");

    try
    {
      using var response =
          await _httpClient.SendAsync(httpRequest);

      var responseBody =
          await response.Content.ReadAsStringAsync();

      _logger.LogInformation(
          "[Hikvision] Create face response. FPID: {Fpid}, Status: {StatusCode}, Response: {Response}",
          request.Fpid,
          (int)response.StatusCode,
          responseBody);

      response.EnsureSuccessStatusCode();

      var result =
          JsonSerializer.Deserialize<CreateUserResponse>(
              responseBody);

      if (result is null)
      {
        throw new Exception(
            "Resposta inválida da Hikvision.");
      }

      if (!result.Success)
      {
        throw new Exception(
            result.ErrorMessage ??
            result.SubStatusCode ??
            result.StatusString);
      }

      _logger.LogInformation(
          "[Hikvision] Face record created successfully. FPID: {Fpid}",
          request.Fpid);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[Hikvision] Failed to create face record. FPID: {Fpid}, FaceURL: {FaceUrl}",
          request.Fpid,
          request.FaceUrl);

      throw;
    }
  }
}
