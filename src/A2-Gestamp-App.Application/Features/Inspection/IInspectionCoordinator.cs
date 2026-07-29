
using A2GestampApp.Application.Features.Keyence.Models;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2GestampApp.Application.Features.Inspection;

public interface IInspectionCoordinator
{
  public event Action<DomainInspection>? InspectionCompleted;

  public void Process(CameraInspectionResult result);

  public void Process(CameraImage image);
}
