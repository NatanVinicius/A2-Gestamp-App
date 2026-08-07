namespace A2GestampApp.Application.Features.Export;

using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Enums;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public sealed class HistoryPdfExportService : IHistoryExportPdfService
{
  [Obsolete]
  public async Task ExportProductionsAsync(
    IReadOnlyCollection<ProductionShift> productions,
    DateOnly? date,
    string shift,
    string filePath)
  {
    QuestPDF.Settings.License = LicenseType.Community;

    var logo = Path.Combine(
        AppContext.BaseDirectory,
        "assets",
        "logo_gestamp.png");

    Document.Create(container =>
    {
      container.Page(page =>
      {
        page.Size(PageSizes.A4);

        page.Margin(20);

        page.DefaultTextStyle(x =>
            x.FontSize(10));

        // HEADER

        page.Header().Column(column =>
        {
          column.Item()
              .Width(120)
              .Image(logo);

          column.Item()
              .PaddingTop(10)
              .AlignCenter()
              .Text("RELATÓRIO DE PRODUÇÃO")
              .Bold()
              .FontSize(18);

          column.Item().PaddingTop(15);

          column.Item().Text($"Data: {(date is null ? "Todas" : date.Value.ToString("dd/MM/yyyy"))}");

          column.Item().Text($"Turno: {shift}");

          column.Item().PaddingTop(10);

          column.Item()
              .LineHorizontal(1)
              .LineColor(Colors.Grey.Lighten2);
        });

        // CONTENT

        page.Content().PaddingTop(30).Table(table =>
        {
          table.ColumnsDefinition(columns =>
          {
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
          });

          table.Header(header =>
          {
            header.Cell().Element(HeaderCell).Text("Data").Bold();

            header.Cell().Element(HeaderCell).Text("Turno").Bold();

            header.Cell().Element(HeaderCell).AlignRight().Text("Produzidas").Bold();

            header.Cell().Element(HeaderCell).AlignRight().Text("Aprovadas").Bold();

            header.Cell().Element(HeaderCell).AlignRight().Text("Reprovadas").Bold();

            header.Cell().Element(HeaderCell).AlignRight().Text("Taxa de Rejeito").Bold();
          });

          foreach (var production in productions)
          {
            table.Cell()
                .Element(DataCell)
                .Text(production.StartDate.ToString("dd/MM/yyyy"));

            table.Cell()
                .Element(DataCell)
                .Text(GetShift(production.ShiftNumber));

            table.Cell()
                .Element(DataCell)
                .AlignRight()
                .Text(production.Produced.ToString());

            table.Cell()
                .Element(DataCell)
                .AlignRight()
                .Text(production.Approved.ToString());

            table.Cell()
                .Element(DataCell)
                .AlignRight()
                .Text(production.Reproved.ToString());

            table.Cell()
                .Element(DataCell)
                .AlignRight()
                .Text($"{production.RejectionRate:F2}%");
          }
        });

        // FOOTER

        page.Footer().Column(column =>
        {
          column.Item()
              .LineHorizontal(0.3f)
.LineColor(Colors.Grey.Lighten2);

          column.Item()
              .PaddingTop(8)
              .AlignCenter()
              .Text("A2 Vision Experts")
              .FontSize(9)
              .FontColor(Colors.Grey.Medium);
        });
      });
    }).GeneratePdf(filePath);
  }

  public async Task ExportInspectionsAsync(
    IReadOnlyCollection<Inspection> inspections,
    DateOnly? date,
    string shift,
    string filePath)
  {
    QuestPDF.Settings.License = LicenseType.Community;

    var logo = Path.Combine(
        AppContext.BaseDirectory,
        "assets",
        "logo_gestamp.png");


    Document.Create(container =>
    {
      container.Page(page =>
      {
        page.Size(PageSizes.A4);

        page.Margin(20);

        page.DefaultTextStyle(x =>
            x.FontSize(10));

        page.Header().Column(column =>
        {
          column.Item()
              .Width(120)
              .Image(logo);

          column.Item()
              .PaddingTop(10)
              .AlignCenter()
              .Text("RELATÓRIO DE INSPEÇÕES")
              .Bold()
              .FontSize(18);

          column.Item().PaddingTop(15);

          column.Item().Text($"Data: {(date is null ? "Todas" : date.Value.ToString("dd/MM/yyyy"))}");

          column.Item().Text($"Turno: {shift}");

          column.Item().PaddingTop(10);

          column.Item()
              .LineHorizontal(1)
              .LineColor(Colors.Grey.Lighten2);
        });

        page.Content().PaddingTop(30).Table(table =>
        {
          table.ColumnsDefinition(columns =>
          {
            columns.RelativeColumn(2);
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn(2);
            columns.RelativeColumn();
          });

          table.Header(header =>
          {
            header.Cell().Element(HeaderCell).Text("Data/Hora").Bold();

            header.Cell().Element(HeaderCell).Text("Original").Bold();

            header.Cell().Element(HeaderCell).Text("Final").Bold();

            header.Cell().Element(HeaderCell).Text("Operador").Bold();

            header.Cell().Element(HeaderCell).AlignCenter().Text("Ciclo").Bold();
          });

          foreach (var inspection in inspections)
          {
            table.Cell()
                .Element(DataCell)
                .Text(inspection.Date.ToString("dd/MM/yyyy HH:mm:ss"));

            table.Cell()
                .Element(DataCell)
                .Text(inspection.OriginalJudgement.ToString());

            table.Cell()
                .Element(DataCell)
                .Text(inspection.FinalJudgement.ToString());

            table.Cell()
                .Element(DataCell)
                .Text(inspection.OperatorName ?? "-");

            table.Cell()
                .Element(DataCell)
                .AlignCenter()
                .Text($"{inspection.CycleTime.TotalMilliseconds:F0} ms");
          }
        });

        page.Footer().Column(column =>
        {
          column.Item()
              .LineHorizontal(0.5f)
              .LineColor(Colors.Grey.Lighten2);

          column.Item()
              .PaddingTop(6)
              .AlignCenter()
              .Text("A2 Vision Experts")
              .FontSize(9)
              .FontColor(Colors.Grey.Medium);
        });
      });
    }).GeneratePdf(filePath);
  }

  private static string GetShift(
      ProductionShiftNumber shift)
  {
    return shift switch
    {
      ProductionShiftNumber.Morning => "Manhã",
      ProductionShiftNumber.Afternoon => "Tarde",
      _ => "-"
    };
  }
  private static IContainer HeaderCell(IContainer container)
  {
    return container
        .Background(Colors.Grey.Lighten3)
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten2)
        .PaddingVertical(8)
        .PaddingHorizontal(10);
  }

  private static IContainer DataCell(IContainer container)
  {
    return container
        .BorderBottom(0.5f)
        .BorderColor(Colors.Grey.Lighten2)
        .PaddingVertical(8)
        .PaddingHorizontal(10);
  }

}
