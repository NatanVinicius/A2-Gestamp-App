namespace Features.Inspection.Domain;

public interface IInspectionService
{
  public Inspection? CurrentInspection { get; }

  public void StartInspection();

  public void AddCameraResult(CameraInspection cameraInspection);

  public void AddCameraImage(int cameraId, byte[] image);
}
