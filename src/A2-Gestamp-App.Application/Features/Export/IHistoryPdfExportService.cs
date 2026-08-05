
using A2GestampApp.Domain.Features.ProductionShift.Entities;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2GestampApp.Application.Features.Export;

public interface IHistoryExportPdfService
{
  public Task ExportProductionsAsync(
    IReadOnlyCollection<ProductionShift> productions,
    DateOnly? date,
    string shift,
    string filePath);

  public Task ExportInspectionsAsync(
      IReadOnlyCollection<DomainInspection> inspections,
      DateOnly? date,
      string shift,
      string filePath);
}
