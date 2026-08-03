using System.Text.Json.Serialization;

namespace A2GestampApp.Infrastructure.Hikvision.Models;

internal sealed class AccessControllerEvent
{
  [JsonPropertyName("subEventType")]
  public int SubEventType { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("employeeNoString")]
  public string? EmployeeNoString { get; init; }
}
