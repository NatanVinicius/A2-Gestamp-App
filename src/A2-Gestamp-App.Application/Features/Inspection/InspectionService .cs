using System.Diagnostics;

using Features.Inspection.Domain;

public sealed class InspectionService : IInspectionService
{
  private Inspection? _currentInspection;

  public Inspection? CurrentInspection => _currentInspection;

  public event EventHandler<Inspection>? InspectionChanged;

  public event EventHandler<Inspection>? InspectionCompleted;

  public void StartInspection()
  {
    Debug.WriteLine("[SERVICE] StartInspectionModel()");

    if (_currentInspection is not null &&
        !_currentInspection.IsComplete())
    {
      throw new InvalidOperationException(
          "Já existe uma inspeção em andamento.");
    }

    _currentInspection = new Inspection();

    InspectionChanged?.Invoke(this, (Inspection)_currentInspection);
  }

  public void AddCameraResult(CameraInspection cameraInspection)
  {
    Debug.WriteLine($"[SERVICE] AddCameraResult Camera {cameraInspection.CameraId}");

    Inspection inspection = CurrentInspectionOrThrow();

    inspection.AddCameraResult(cameraInspection);

    InspectionChanged?.Invoke(this, inspection);
  }

  public void AddCameraImage(int cameraId, byte[] image)
  {
    Inspection inspection = CurrentInspectionOrThrow();

    inspection.AddCameraImage(cameraId, image);

    InspectionChanged?.Invoke(this, inspection);

    if (!inspection.IsComplete())
    {
      return;
    }

    inspection.Finish();

    Debug.WriteLine($"[SERVICE] Inspection finished ({inspection.Judgement})");

    InspectionCompleted?.Invoke(this, inspection);

    // Mantém a inspeção disponível até a próxima iniciar.
    InspectionChanged?.Invoke(this, inspection);
  }

  private Inspection CurrentInspectionOrThrow()
  {
    return _currentInspection
        ?? throw new InvalidOperationException(
            "Nenhuma inspeção está ativa.");
  }
}
