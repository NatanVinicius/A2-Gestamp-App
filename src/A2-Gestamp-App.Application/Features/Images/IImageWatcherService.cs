

public interface IImageWatcherService
{
  public event Action<CameraImage>? ImageReceived;

  public void Start();
}
