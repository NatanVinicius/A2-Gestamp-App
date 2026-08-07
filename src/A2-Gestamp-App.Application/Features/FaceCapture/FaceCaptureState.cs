using A2GestampApp.Infrastructure.Hikvision;

namespace A2GestampApp.Application.Features.FaceCapture;

public sealed class FaceCaptureState
    : IFaceCaptureState
{
  private readonly IHikvisionUserService _hikvisionUserService;

  public FaceCaptureState(
      IHikvisionUserService hikvisionUserService)
  {
    _hikvisionUserService = hikvisionUserService;
  }

  public event Action? StateChanged;

  public bool IsOpen { get; private set; }

  public FaceCaptureStatus Status { get; private set; }
      = FaceCaptureStatus.Waiting;

  public int RemainingSeconds { get; private set; }

  public void Open()
  {
    RemainingSeconds = 10;

    Status = FaceCaptureStatus.Waiting;

    IsOpen = true;

    NotifyStateChanged();
  }

  public void Close()
  {
    IsOpen = false;

    RemainingSeconds = 0;

    Status = FaceCaptureStatus.Waiting;

    NotifyStateChanged();
  }

  public async Task StartCaptureAsync(
    string employeeId,
    string name,
    UserRole role)
  {
    if (Status != FaceCaptureStatus.Waiting)
    {
      return;
    }

    try
    {
      Status = FaceCaptureStatus.Capturing;

      NotifyStateChanged();

      while (RemainingSeconds > 0)
      {
        await Task.Delay(1000);

        RemainingSeconds--;

        NotifyStateChanged();
      }

      await _hikvisionUserService.RegisterAsync(
          employeeId,
          name,
          role);

      Status = FaceCaptureStatus.Success;

      NotifyStateChanged();

      await Task.Delay(1500);

      Close();
    }
    catch (Exception)
    {
      Status = FaceCaptureStatus.Failed;

      NotifyStateChanged();

      await Task.Delay(2000);

      RemainingSeconds = 10;

      Status = FaceCaptureStatus.Waiting;

      NotifyStateChanged();
    }
  }

  private void NotifyStateChanged()
  {
    StateChanged?.Invoke();
  }
}
