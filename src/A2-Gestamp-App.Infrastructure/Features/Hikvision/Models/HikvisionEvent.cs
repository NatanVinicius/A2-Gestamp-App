using System.Text.Json.Serialization;

namespace A2GestampApp.Infrastructure.Hikvision.Models;

internal sealed class HikvisionEvent
{
  [JsonPropertyName("AccessControllerEvent")]
  public AccessControllerEvent AccessControllerEvent { get; init; } = default!;
}
