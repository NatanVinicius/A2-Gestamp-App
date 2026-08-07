using System.Text.Json.Serialization;

namespace A2GestampApp.Infrastructure.Hikvision.Models.Responses;

public sealed class CreateUserResponse
{
  [JsonPropertyName("statusCode")]
  public int StatusCode { get; init; }

  [JsonPropertyName("statusString")]
  public string StatusString { get; init; } = string.Empty;

  [JsonPropertyName("subStatusCode")]
  public string? SubStatusCode { get; init; }

  [JsonPropertyName("errorMsg")]
  public string? ErrorMessage { get; init; }

  [JsonIgnore]
  public bool Success =>
      StatusCode == 1 ||
      StatusString.Equals("OK", StringComparison.OrdinalIgnoreCase);
}
