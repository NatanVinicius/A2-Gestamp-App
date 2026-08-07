namespace A2GestampApp.Application.Features.AdminAuthentication;

public sealed class AdminAuthenticationState : IAdminAuthenticationState
{
  public bool IsOpen { get; private set; }

  public FaceRecognitionStatus Status { get; private set; }
      = FaceRecognitionStatus.Waiting;

  public int RemainingSeconds { get; private set; } = 10;

  public event Action? StateChanged;

  private readonly IFaceRecognitionService _faceRecognitionService;

  public AdminAuthenticationState(
      IFaceRecognitionService faceRecognitionService)
  {
    _faceRecognitionService = faceRecognitionService;
  }

  public void Open()
  {
    IsOpen = true;

    Status = FaceRecognitionStatus.Waiting;

    RemainingSeconds = 10;

    StateChanged?.Invoke();
  }

  public async void Close()
  {
    await _faceRecognitionService.DisableAsync();

    IsOpen = false;

    Status = FaceRecognitionStatus.Waiting;

    RemainingSeconds = 10;

    StateChanged?.Invoke();
  }

  public async Task StartRecognitionAsync()
  {
    if (Status == FaceRecognitionStatus.Recognizing)
    {
      return;
    }

    Status = FaceRecognitionStatus.Recognizing;

    RemainingSeconds = 10;

    StateChanged?.Invoke();

    await _faceRecognitionService.EnableAsync();

    while (RemainingSeconds > 0)
    {
      await Task.Delay(1000);

      if (Status != FaceRecognitionStatus.Recognizing)
      {
        return;
      }

      RemainingSeconds--;

      StateChanged?.Invoke();
    }

    await _faceRecognitionService.DisableAsync();

    Status = FaceRecognitionStatus.Waiting;

    RemainingSeconds = 10;

    StateChanged?.Invoke();
  }

  public async Task AuthenticateAsync(UserRole role)
  {
    await _faceRecognitionService.DisableAsync();

    if (role != UserRole.AuxiliaryManager)
    {
      Status = FaceRecognitionStatus.Failed;

      StateChanged?.Invoke();

      return;
    }

    Status = FaceRecognitionStatus.Success;

    StateChanged?.Invoke();
  }

  public void Logout()
  {
    Close();
  }
}
