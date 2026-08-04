using A2GestampApp.Domain.Features.Inspection.Enums;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2GestampApp.Application.Features.Inspection.Services;

internal sealed class InspectionReviewService : IInspectionReviewService
{
  private readonly IInspectionRepository _inspectionRepository;
  private readonly IProductionShiftRepository _productionShiftRepository;
  private readonly IInspectionState _inspectionState;
  private readonly IProductionShiftState _productionShiftState;

  public InspectionReviewService(
      IInspectionRepository inspectionRepository,
      IProductionShiftRepository productionShiftRepository,
      IInspectionState inspectionState,
      IProductionShiftState productionShiftState)
  {
    _inspectionRepository = inspectionRepository;
    _productionShiftRepository = productionShiftRepository;
    _inspectionState = inspectionState;
    _productionShiftState = productionShiftState;
  }

  public async Task SaveReviewAsync(
    DomainInspection inspection,
    FaceRecognitionEvent user,
    CancellationToken cancellationToken = default)
  {
    inspection.SetReviewer(
        user.Name,
        user.EmployeeNumber,
        (int)user.Role);

    await _inspectionRepository.UpdateAsync(
        inspection,
        cancellationToken);

    _inspectionState.SetInspection(inspection);
  }

  public async Task ApproveAsync(
    DomainInspection inspection,
    FaceRecognitionEvent user,
    CancellationToken cancellationToken = default)
  {
    InspectionResult previousJudgement =
        inspection.FinalJudgement;

    inspection.Approve();

    await SaveReviewAsync(
        inspection,
        user,
        cancellationToken);

    _productionShiftState.CurrentShift.ChangeJudgement(
        previousJudgement,
        inspection.FinalJudgement);

    await _productionShiftRepository.UpdateAsync(
        _productionShiftState.CurrentShift,
        cancellationToken);

    _productionShiftState.NotifyStateChanged();
  }

}
