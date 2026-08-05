using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Domain.Features.Inspection.Enums;

namespace A2GestampApp.Domain.Features.Inspection.Entities;

public sealed class Inspection
{
  public int Id { get; private set; }

  public DateTime Date { get; private set; }

  public string? OperatorName { get; private set; }

  public string? EmployeeNumber { get; private set; }

  public int? OperatorRole { get; private set; }
  public CameraInspection Camera1 { get; } = new(1);

  public CameraInspection Camera2 { get; } = new(2);

  public CameraInspection Camera3 { get; } = new(3);

  public string FirstImagePath { get; private set; } = string.Empty;

  public string SecondImagePath { get; private set; } = string.Empty;

  public string ThirdImagePath { get; private set; } = string.Empty;

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


  public TimeSpan CycleTime { get; private set; }

  public int? ProductionShiftId { get; private set; }

  public Inspection()
  {
    Date = DateTime.Now;
  }

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

    FirstImagePath = Camera1.ImagePath;
    SecondImagePath = Camera2.ImagePath;
    ThirdImagePath = Camera3.ImagePath;

    CycleTime = new[]
    {
        Camera1.ExecutionTime,
        Camera2.ExecutionTime,
        Camera3.ExecutionTime
    }.Max();

    if (FinalJudgement == default)
    {
      FinalJudgement = OriginalJudgement;
    }
  }

  public void SetReviewer(
    string name,
    string employeeNumber,
    int role)
  {
    OperatorName = name;
    EmployeeNumber = employeeNumber;
    OperatorRole = role;
  }

  public void LinkToProductionShift(
    int productionShiftId)
  {
    ProductionShiftId = productionShiftId;
  }
}
