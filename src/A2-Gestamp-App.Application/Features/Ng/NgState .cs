using A2GestampApp.Application.Features.Ng;

public sealed class NgState : INgState
{
  public bool IsOpen { get; private set; }

  public event Action? StateChanged;

  public FaceRecognitionStatus Status { get; private set; }
    = FaceRecognitionStatus.Waiting;

  public int RemainingSeconds { get; private set; } = 10;

  private readonly IFaceRecognitionService _faceRecognitionService;

  public NgState(IFaceRecognitionService faceRecognitionService)
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

  public void Close()
  {
    IsOpen = false;
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

  public Task SetSuccessAsync()
  {
    Status = FaceRecognitionStatus.Success;

    StateChanged?.Invoke();

    return Task.CompletedTask;
  }
}
