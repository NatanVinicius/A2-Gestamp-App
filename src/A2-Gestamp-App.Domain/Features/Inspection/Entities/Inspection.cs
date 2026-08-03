using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Domain.Features.Inspection.Enums;

namespace A2GestampApp.Domain.Features.Inspection.Entities;

public sealed class Inspection
{
  public CameraInspection Camera1 { get; } = new(1);

  public CameraInspection Camera2 { get; } = new(2);

  public CameraInspection Camera3 { get; } = new(3);

  public InspectionResult OriginalJudgement { get; private set; }

  public InspectionResult FinalJudgement { get; private set; }

  public bool IsCompleted =>
      Camera1.IsCompleted &&
      Camera2.IsCompleted &&
      Camera3.IsCompleted;

  public bool Approved =>
      Camera1.Approved &&
      Camera2.Approved &&
      Camera3.Approved;


  public TimeSpan CycleTime =>
      new[]
      {
            Camera1.ExecutionTime,
            Camera2.ExecutionTime,
            Camera3.ExecutionTime
      }.Max();

  private InspectionResult CalculateOriginalJudgement()
  {
    return Approved
        ? InspectionResult.Aprovada
        : InspectionResult.Reprovada;
  }

  public CameraInspection GetCamera(int cameraId)
  {
    return cameraId switch
    {
      1 => Camera1,
      2 => Camera2,
      3 => Camera3,
      _ => throw new ArgumentOutOfRangeException(nameof(cameraId))
    };
  }

  public void Approve()
  {
    FinalJudgement = InspectionResult.Aprovada;
  }

  public void UpdateJudgements()
  {
    OriginalJudgement = CalculateOriginalJudgement();

    if (FinalJudgement == default)
    {
      FinalJudgement = OriginalJudgement;
    }
  }
}
