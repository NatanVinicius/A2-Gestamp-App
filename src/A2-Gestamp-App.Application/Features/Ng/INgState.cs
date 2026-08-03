namespace A2GestampApp.Application.Features.Ng;

public interface INgState
{
  public bool IsOpen { get; }

  public event Action? StateChanged;

  public void Open();

  public void Close();

  public FaceRecognitionStatus Status { get; }

  public int RemainingSeconds { get; }

  public Task StartRecognitionAsync();

  public Task SetSuccessAsync();
}
