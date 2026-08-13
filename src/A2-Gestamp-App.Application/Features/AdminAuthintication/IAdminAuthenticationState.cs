namespace A2GestampApp.Application.Features.AdminAuthentication;

public interface IAdminAuthenticationState
{
  public bool IsOpen { get; }

  public FaceRecognitionStatus Status { get; }

  public int RemainingSeconds { get; }

  public AdminAuthenticationPurpose Purpose { get; }

  public event Action? StateChanged;

  public void Open(
      AdminAuthenticationPurpose purpose = AdminAuthenticationPurpose.Signup);

  public void Close();

  public Task StartRecognitionAsync();

  public Task AuthenticateAsync(UserRole role);

  public void Logout();
}
