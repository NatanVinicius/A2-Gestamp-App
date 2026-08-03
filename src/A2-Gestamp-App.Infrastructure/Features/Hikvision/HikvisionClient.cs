using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace A2GestampApp.Infrastructure.Hikvision;

public sealed class HikvisionClient
{
  private readonly HttpClient _httpClient;

  public HikvisionClient()
  {
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

    using var request = new HttpRequestMessage(
        HttpMethod.Put,
        $"http://{deviceAddress}/ISAPI/Event/notification/httpHosts/1");

    request.Content = new StringContent(
        xml,
        Encoding.UTF8,
        "application/xml");

    using var response = await _httpClient.SendAsync(request);

    var body = await response.Content.ReadAsStringAsync();

    Debug.WriteLine(body);

    response.EnsureSuccessStatusCode();
  }

  public async Task<User?> SearchUserAsync(
      string deviceAddress,
      string employeeNumber)
  {
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
        $"http://{deviceAddress}/ISAPI/AccessControl/UserInfo/Search?format=json");

    request.Content = new StringContent(
        body,
        Encoding.UTF8,
        "application/json");

    using var response = await _httpClient.SendAsync(request);

    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadAsStringAsync();

    using var document = JsonDocument.Parse(json);

    if (!document.RootElement.TryGetProperty("UserInfoSearch", out var search))
    {
      return null;
    }

    if (!search.TryGetProperty("UserInfo", out var users))
    {
      return null;
    }

    if (users.ValueKind != JsonValueKind.Array || users.GetArrayLength() == 0)
    {
      return null;
    }

    var user = users[0];

    var name = user.TryGetProperty("name", out var nameProperty)
        ? nameProperty.GetString() ?? string.Empty
        : string.Empty;

    var userType = user.TryGetProperty("userType", out var typeProperty)
        ? typeProperty.GetString() ?? string.Empty
        : string.Empty;

    return new User
    {
      EmployeeNumber = employeeNumber,
      Name = name,
      Role = userType.Equals("auxiliaryManager", StringComparison.OrdinalIgnoreCase)
            ? UserRole.AuxiliaryManager
            : UserRole.Normal
    };
  }

  public async Task SetCardReaderEnabledAsync(bool enabled)
  {
    using var request = new HttpRequestMessage(
        HttpMethod.Put,
        "http://192.168.70.40/ISAPI/AccessControl/CardReaderCfg/1?format=json");

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

    Debug.WriteLine(json);

    var response = await _httpClient.SendAsync(request);

    var body = await response.Content.ReadAsStringAsync();

    Debug.WriteLine(response.StatusCode);
    Debug.WriteLine(body);

    response.EnsureSuccessStatusCode();
  }
}
