using System.Text.Json.Serialization;

namespace A2GestampApp.Infrastructure.Hikvision.Models.Requests;

public sealed class CreateFaceRecordRequest
{
  [JsonPropertyName("faceURL")]
  public string FaceUrl { get; init; } = string.Empty;

  [JsonPropertyName("faceLibType")]
  public string FaceLibType { get; init; } = "blackFD";

  [JsonPropertyName("FDID")]
  public string Fdid { get; init; } = "1";

  [JsonPropertyName("FPID")]
  public string Fpid { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("bornTime")]
  public string BornTime { get; init; } = "1995-01-01";
}
