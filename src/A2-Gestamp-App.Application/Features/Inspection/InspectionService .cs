using System.Diagnostics;

namespace Features.Inspection.Domain;

public sealed class InspectionService : IInspectionService
{
  private Inspection? _currentInspection;

  public Inspection? CurrentInspection => _currentInspection;

  public event EventHandler<Inspection>? InspectionCompleted;

  public void StartInspection()
  {
    Debug.WriteLine("[SERVICE] StartInspection()");

    if (_currentInspection is not null &&
        !_currentInspection.IsComplete())
    {
      throw new InvalidOperationException(
          "Já existe uma inspeção em andamento.");
    }

    _currentInspection = new Inspection();
  }

  public void AddCameraResult(CameraInspection cameraInspection)
  {
    Debug.WriteLine($"[SERVICE] AddCameraResult Camera {cameraInspection.CameraId}");

    CurrentInspectionOrThrow()
        .AddCameraResult(cameraInspection);
  }


  public void AddCameraImage(int cameraId, byte[] image)
  {
    Inspection inspection = CurrentInspectionOrThrow();

    inspection.AddCameraImage(cameraId, image);

    if (!inspection.IsComplete())
    {
      return;
    }

    inspection.Finish();

    Debug.WriteLine($"[SERVICE] Inspection finished ({inspection.Judgement})");

    InspectionCompleted?.Invoke(this, inspection);

    _currentInspection = null;
  }

  private Inspection CurrentInspectionOrThrow()
  {
    return _currentInspection
        ?? throw new InvalidOperationException(
            "Nenhuma inspeção está ativa.");
  }
}
