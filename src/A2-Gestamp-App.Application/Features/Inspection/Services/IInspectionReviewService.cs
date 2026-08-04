using A2GestampApp.Domain.Features.Inspection.Entities;

public interface IInspectionReviewService
{
  public Task SaveReviewAsync(
      Inspection inspection,
      FaceRecognitionEvent user,
      CancellationToken cancellationToken = default);

  public Task ApproveAsync(
      Inspection inspection,
      FaceRecognitionEvent user,
      CancellationToken cancellationToken = default);
}
