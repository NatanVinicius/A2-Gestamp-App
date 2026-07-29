using A2GestampApp.Application.Features.Keyence.Models;

public sealed class CameraInspectionState
{
  public CameraInspectionResult? Result { get; set; }

  public CameraImage? Image { get; set; }

  public bool IsCompleted =>
      Result is not null &&
      Image is not null;
}
