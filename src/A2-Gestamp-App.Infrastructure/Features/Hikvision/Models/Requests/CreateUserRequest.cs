using System.Text.Json.Serialization;

namespace A2GestampApp.Infrastructure.Hikvision.Models.Requests;

public sealed class CreateUserRequest
{
  [JsonPropertyName("UserInfo")]
  public UserInfo UserInfo { get; init; } = new();
}

public sealed class UserInfo
{
  [JsonPropertyName("employeeNo")]
  public string EmployeeNo { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("userType")]
  public string UserType { get; init; } = "normal";

  [JsonPropertyName("userVerifyMode")]
  public string UserVerifyMode { get; init; } = "face";

  [JsonPropertyName("Valid")]
  public ValidPeriod Valid { get; init; } = new();

  [JsonPropertyName("doorRight")]
  public string DoorRight { get; init; } = "1";

  [JsonPropertyName("RightPlan")]
  public List<RightPlan> RightPlan { get; init; } =
  [
      new()
  ];
}

public sealed class ValidPeriod
{
  [JsonPropertyName("enable")]
  public bool Enable { get; init; } = true;

  [JsonPropertyName("beginTime")]
  public string BeginTime { get; init; } =
      "2024-01-01T00:00:00";

  [JsonPropertyName("endTime")]
  public string EndTime { get; init; } =
      "2030-12-31T23:59:59";

  [JsonPropertyName("timeType")]
  public string TimeType { get; init; } = "local";
}

public sealed class RightPlan
{
  [JsonPropertyName("doorNo")]
  public int DoorNo { get; init; } = 1;

  [JsonPropertyName("planTemplateNo")]
  public string PlanTemplateNo { get; init; } = "1";
}
