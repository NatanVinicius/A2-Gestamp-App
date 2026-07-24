using Features.Inspection.Domain;

public sealed class CameraInspection
{
  public int CameraId { get; }

  public bool Passed { get; }

  public double ExecutionTime { get; }

  public IReadOnlyList<ToolResult> Tools { get; }

  public byte[]? Image { get; private set; }

  public CameraInspection(
    int cameraId,
    bool passed,
    double executionTime,
    IReadOnlyList<ToolResult> tools)
  {
    CameraId = cameraId;
    Passed = passed;
    ExecutionTime = executionTime;
    Tools = tools;
  }

  public void SetImage(byte[] image)
  {
    ArgumentNullException.ThrowIfNull(image);

    Image = image;
  }
}
