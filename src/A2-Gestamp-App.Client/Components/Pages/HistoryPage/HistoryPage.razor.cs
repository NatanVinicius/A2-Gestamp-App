namespace A2GestampApp.Client.Components.Pages.HistoryPage;

using System.Diagnostics;

using A2GestampApp.Application.Features.Export;
using A2GestampApp.Domain.Features.ProductionShift.Entities;
using A2GestampApp.Domain.Features.ProductionShift.Enums;

using CommunityToolkit.Maui.Storage;

using Microsoft.AspNetCore.Components;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

public partial class HistoryPage : ComponentBase
{

  [Inject]
  protected IProductionShiftRepository ProductionShiftRepository { get; set; } = default!;

  [Inject]
  protected IInspectionRepository InspectionRepository { get; set; } = default!;

  [Inject]
  protected IHistoryExportPdfService HistoryPdfService { get; set; } = default!;

  private HistoryTable _selectedTable = HistoryTable.Production;

  protected HistoryTable SelectedTable
  {
    get => _selectedTable;

    set
    {
      if (_selectedTable == value)
      {
        return;
      }

      _selectedTable = value;

      _ = LoadAsync();
    }
  }

  private DateTime _selectedDate = DateTime.Today;

  protected DateTime SelectedDate
  {
    get => _selectedDate;

    set
    {
      if (_selectedDate == value)
      {
        return;
      }

      _selectedDate = value;

      _ = LoadAsync();
    }
  }

  protected ShiftFilter SelectedShift { get; set; } = ShiftFilter.All;

  protected enum HistoryTable
  {
    Production,
    Inspection
  }

  protected enum ShiftFilter
  {
    All,
    Morning,
    Afternoon
  }

  protected List<ProductionShift> Productions { get; private set; } = [];

  protected List<DomainInspection> Inspections { get; private set; } = [];

  protected override async Task OnInitializedAsync()
  {
    await LoadAsync();
  }

  private async Task LoadAsync()
  {
    if (SelectedTable == HistoryTable.Production)
    {
      await LoadProductionsAsync();
    }
    else
    {
      await LoadInspectionsAsync();
    }
  }

  private async Task LoadProductionsAsync()
  {
    Productions = await ProductionShiftRepository.GetAsync(
        SelectedDate == DateTime.MinValue
            ? null
            : DateOnly.FromDateTime(SelectedDate),

        GetSelectedShift());
  }

  private async Task LoadInspectionsAsync()
  {
    Inspections = await InspectionRepository.GetAsync(
        SelectedDate == DateTime.MinValue
            ? null
            : DateOnly.FromDateTime(SelectedDate),

        GetSelectedShift());
  }

  private ProductionShiftNumber? GetSelectedShift()
  {
    return SelectedShift switch
    {
      ShiftFilter.All => null,
      ShiftFilter.Morning => ProductionShiftNumber.Morning,
      ShiftFilter.Afternoon => ProductionShiftNumber.Afternoon,
      _ => null
    };
  }

  protected static string GetShiftName(ProductionShiftNumber shift) =>
    shift switch
    {
      ProductionShiftNumber.Morning => "Manhã",
      ProductionShiftNumber.Afternoon => "Tarde",
      _ => "-"
    };

  private async Task OnFiltersChanged()
  {
    await LoadAsync();
  }

  protected async Task ExportPdfAsync()
  {
    var shift = SelectedShift switch
    {
      ShiftFilter.All => "Todos",
      ShiftFilter.Morning => "Manhã",
      ShiftFilter.Afternoon => "Tarde",
      _ => "Todos"
    };

    DateOnly? date = SelectedDate == DateTime.MinValue
        ? null
        : DateOnly.FromDateTime(SelectedDate);

    var fileName = SelectedTable == HistoryTable.Production
        ? $"Production_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        : $"Inspection_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

    var tempFile = Path.Combine(FileSystem.CacheDirectory, fileName);

    if (SelectedTable == HistoryTable.Production)
    {
      await HistoryPdfService.ExportProductionsAsync(
          Productions,
          date,
          shift,
          tempFile);
    }
    else
    {
      await HistoryPdfService.ExportInspectionsAsync(
          Inspections,
          date,
          shift,
          tempFile);
    }

    await using var stream = File.OpenRead(tempFile);

    await FileSaver.Default.SaveAsync(
        fileName,
        stream,
        CancellationToken.None);
  }

  private void OpenRejectFolder(DomainInspection inspection)
  {
    if (string.IsNullOrWhiteSpace(inspection.RejectFolder))
    {
      return;
    }

    var folder = Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "Rejeitos",
        inspection.RejectFolder);

    if (!Directory.Exists(folder))
    {
      return;
    }

    Process.Start(new ProcessStartInfo
    {
      FileName = folder,
      UseShellExecute = true
    });
  }

}
