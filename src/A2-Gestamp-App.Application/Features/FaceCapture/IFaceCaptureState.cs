public interface IFaceCaptureState
{
  public bool IsOpen { get; }

  public int RemainingSeconds { get; }

  public FaceCaptureStatus Status { get; }

  public event Action? StateChanged;

  public void Open();

  public void Close();

  public Task StartCaptureAsync(
      string employeeId,
      string name,
      UserRole role);
}
