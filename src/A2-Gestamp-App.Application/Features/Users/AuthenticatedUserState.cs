public sealed class AuthenticatedUserState : IAuthenticatedUserState
{
  public FaceRecognitionEvent? User { get; private set; }

  public event Action? StateChanged;

  public void SetUser(FaceRecognitionEvent user)
  {
    User = user;
    StateChanged?.Invoke();
  }

  public void Clear()
  {
    User = null;
    StateChanged?.Invoke();
  }
}
