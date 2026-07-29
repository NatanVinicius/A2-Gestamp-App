
using A2GestampApp.Application.Features.Keyence.Models;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Models.Inspection;

namespace A2GestampApp.Application.Features.Inspection.IInspection;

public interface IInspectionCoordinator
{
  public event Action<DomainInspection>? InspectionCompleted;

  public void Process(CameraInspectionResult result);

  public void Process(CameraImage image);
}
