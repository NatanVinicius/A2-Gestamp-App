public interface IFaceRecognitionService
{
  public event Action<FaceRecognitionEvent>? UserRecognized;

  public Task StartAsync();

  public Task EnableAsync();

  public Task DisableAsync();
}
