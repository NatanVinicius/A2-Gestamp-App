using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2GestampApp.Application.Features.Images.Services;

public interface IImageTransferService
{
  public void Transfer(DomainInspection inspection);
}
