using System.Diagnostics;

using Features.Inspection.Domain.Enums;

namespace Features.Inspection.Domain;

public sealed class Inspection
{
  private const int RequiredCameras = 3;

  private readonly List<CameraInspection> _cameras = [];
  public Guid Id { get; } = Guid.NewGuid();

  public DateTime CreatedAt { get; } = DateTime.UtcNow;

  public InspectionStatus Status { get; private set; } = InspectionStatus.Created;

  public InspectionJudgement Judgement { get; private set; } = InspectionJudgement.Unknown;

  public IReadOnlyList<CameraInspection> Cameras =>
    _cameras
        .OrderBy(c => c.CameraId)
        .ToList();

  public void AddCameraResult(CameraInspection cameraInspection)
  {
    Debug.WriteLine($"[DOMAIN] Adicionando câmera {cameraInspection.CameraId}");

    ArgumentNullException.ThrowIfNull(cameraInspection);

    if (_cameras.Any(c => c.CameraId == cameraInspection.CameraId))
    {
      throw new InvalidOperationException(
          $"Camera {cameraInspection.CameraId} já possui resultado.");
    }

    _cameras.Add(cameraInspection);

    if (Status == InspectionStatus.Created)
    {
      Status = InspectionStatus.CollectingData;
    }

    TryEvaluate();
  }

  public void AddCameraImage(int cameraId, byte[] image)
  {
    ArgumentNullException.ThrowIfNull(image);

    var camera = _cameras.FirstOrDefault(c => c.CameraId == cameraId)
        ?? throw new InvalidOperationException(
            $"Resultado da câmera {cameraId} ainda não foi recebido.");

    camera.SetImage(image);

    TryEvaluate();
  }

  private void TryEvaluate()
  {
    Debug.WriteLine("[DOMAIN] TryEvaluate()");

    if (Status == InspectionStatus.Evaluated ||
        Status == InspectionStatus.Finished)
    {
      return;
    }

    if (_cameras.Count != RequiredCameras)
    {
      return;
    }

    if (_cameras.Any(c => c.Image is null))
    {
      return;
    }

    Status = InspectionStatus.Evaluated;

    Judgement = _cameras.All(c => c.Passed)
        ? InspectionJudgement.Aprovada
        : InspectionJudgement.Reprovada;

    Debug.WriteLine($"[DOMAIN] Resultado = {Judgement}");
  }

  public bool IsComplete()
  {
    return _cameras.Count == RequiredCameras &&
           _cameras.All(c => c.Image is not null);
  }

  public void Finish()
  {
    if (Status == InspectionStatus.Created ||
        Status == InspectionStatus.CollectingData)
    {
      throw new InvalidOperationException(
          "A inspeção ainda não foi avaliada.");
    }

    Status = InspectionStatus.Finished;
  }


}
