public interface IAuthenticatedUserState
{
  public FaceRecognitionEvent? User { get; }

  public event Action? StateChanged;

  public void SetUser(FaceRecognitionEvent user);

  public void Clear();
}
