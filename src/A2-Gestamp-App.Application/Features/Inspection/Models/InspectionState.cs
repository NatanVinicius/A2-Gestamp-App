namespace A2GestampApp.Application.Features.Inspection.State;

public sealed class InspectionState
{
  public CameraInspectionState Camera1 { get; } = new();

  public CameraInspectionState Camera2 { get; } = new();

  public CameraInspectionState Camera3 { get; } = new();

  public bool IsCompleted =>
      Camera1.IsCompleted &&
      Camera2.IsCompleted &&
      Camera3.IsCompleted;
}
