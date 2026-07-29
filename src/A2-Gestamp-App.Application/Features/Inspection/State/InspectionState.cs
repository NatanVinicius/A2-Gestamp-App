using A2GestampApp.Application.Features.Inspection;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

public sealed class InspectionState : IInspectionState
{
  public DomainInspection? CurrentInspection { get; private set; }

  public event Action? InspectionChanged;

  public void SetInspection(DomainInspection inspection)
  {
    CurrentInspection = inspection;
    InspectionChanged?.Invoke();
  }
}
