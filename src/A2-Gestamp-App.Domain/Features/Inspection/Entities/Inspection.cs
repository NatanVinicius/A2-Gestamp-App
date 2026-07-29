namespace A2GestampApp.Domain.Features.Inspection.Models;

public sealed class Inspection
{
  public CameraInspection Camera1 { get; } = new(1);

  public CameraInspection Camera2 { get; } = new(2);

  public CameraInspection Camera3 { get; } = new(3);

  public bool IsCompleted =>
      Camera1.IsCompleted &&
      Camera2.IsCompleted &&
      Camera3.IsCompleted;

  public CameraInspection GetCamera(int cameraId)
  {
    return cameraId switch
    {
      1 => Camera1,
      2 => Camera2,
      3 => Camera3,
      _ => throw new ArgumentOutOfRangeException(nameof(cameraId))
    };
  }
}
