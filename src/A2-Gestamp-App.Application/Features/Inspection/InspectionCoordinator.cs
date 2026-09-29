using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Application.Features.Keyence.Models;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2GestampApp.Application.Features.Inspection;

public sealed class InspectionCoordinator : IInspectionCoordinator
{
  private readonly object _sync = new();
  private readonly IInspectionState _inspectionState;

  private DomainInspection _inspection = new();

  public event Action<DomainInspection>? InspectionCompleted;

  public InspectionCoordinator(
      IInspectionState inspectionState)
  {
    _inspectionState = inspectionState;
  }

  public void Process(CameraInspectionResult result)
  {
    DomainInspection? completedInspection;

    lock (_sync)
    {
      List<ToolInspection> tools = result.Tools
          .Select(tool => new ToolInspection
          {
            Name = tool.Name,
            Approved = tool.Approved,
            ExecutionTime = tool.ExecutionTime
          })
          .ToList();

      _inspection
          .GetCamera(result.Header.CameraId)
          .SetResult(
              result.Header.Approved,
              result.Header.ExecutionTime,
              tools);

      completedInspection = TryCompleteLocked();
    }

    RaiseCompleted(completedInspection);
  }

  public void Process(CameraImage image)
  {
    DomainInspection? completedInspection;

    lock (_sync)
    {
      _inspection
          .GetCamera(image.CameraId)
          .SetImage(image.ImagePath);

      completedInspection = TryCompleteLocked();
    }

    _inspectionState.NotifyChanged();

    RaiseCompleted(completedInspection);
  }

  private DomainInspection? TryCompleteLocked()
  {
    if (!_inspection.IsCompleted)
    {
      return null;
    }

    _inspection.UpdateJudgements();

    DomainInspection completedInspection = _inspection;

    // IMPORTANT: reset the active inspection before publishing the event.
    // This prevents two concurrent image/result callbacks from completing
    // the same inspection twice.
    _inspection = new DomainInspection();

    return completedInspection;
  }

  private void RaiseCompleted(DomainInspection? inspection)
  {
    if (inspection is null)
    {
      return;
    }

    InspectionCompleted?.Invoke(inspection);
  }
}
