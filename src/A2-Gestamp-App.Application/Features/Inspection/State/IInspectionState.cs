using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2GestampApp.Application.Features.Inspection;

public interface IInspectionState
{
  public DomainInspection? CurrentInspection { get; }

  public event Action? InspectionChanged;

  public void SetInspection(DomainInspection inspection);
}
