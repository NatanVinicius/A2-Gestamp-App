namespace A2GestampApp.Application.Features.Hikvision;

public interface IFaceImageServer : IDisposable
{
  public Task StartAsync();

  public void SetImage(byte[] image);

  public void Clear();
}
