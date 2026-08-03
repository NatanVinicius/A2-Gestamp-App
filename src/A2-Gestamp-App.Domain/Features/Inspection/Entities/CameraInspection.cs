namespace A2_Gestamp_App.Domain.Features.Inspection.Entities;

public sealed class CameraInspection
{
  public int CameraId { get; }

  public bool Approved { get; private set; }

  public TimeSpan ExecutionTime { get; private set; }

  public string? ImagePath { get; private set; }

  public string? OverlayPath { get; private set; }

  public IReadOnlyCollection<ToolInspection> Tools => _tools;

  public bool HasResult { get; private set; }

  public bool HasImage =>
      !string.IsNullOrWhiteSpace(ImagePath);

  public bool IsCompleted =>
      HasResult && HasImage;

  private readonly List<ToolInspection> _tools = [];

  public CameraInspection(int cameraId)
  {
    CameraId = cameraId;
  }

  public void SetResult(
      bool approved,
      TimeSpan executionTime,
      IEnumerable<ToolInspection> tools)
  {
    Approved = approved;
    ExecutionTime = executionTime;

    _tools.Clear();
    _tools.AddRange(tools);

    HasResult = true;
  }

  public void SetImage(string imagePath)
  {
    ImagePath = imagePath;
  }

  public void SetOverlay(string overlayPath)
  {
    OverlayPath = overlayPath;
  }
}
