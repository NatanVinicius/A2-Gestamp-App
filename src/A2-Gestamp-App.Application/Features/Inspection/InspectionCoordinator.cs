using System.Diagnostics;
using System.Text.Json; // <-- Adicionado para serialização

using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Application.Features.Keyence.Models;

using DomainInspection = A2GestampApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2GestampApp.Application.Features.Inspection;

public sealed class InspectionCoordinator : IInspectionCoordinator
{
  private DomainInspection _inspection = new();

  // Opções para formatar o JSON bonitinho no log (opcional)
  private static readonly JsonSerializerOptions _jsonOptions = new()
  {
    WriteIndented = true
  };

  public event Action<DomainInspection>? InspectionCompleted;

  public void Process(CameraInspectionResult result)
  {

    List<ToolInspection> tools = result.Tools
        .Select(tool => new ToolInspection
        {
          Name = tool.Name,
          Approved = tool.Approved,
          ExecutionTime = tool.ExecutionTime
        })
        .ToList();

    _inspection
        .GetCamera(result.Header.CameraId)
        .SetResult(
            result.Header.Approved,
            result.Header.ExecutionTime,
            tools);

    // Serializa o objeto Inspection inteiro para JSON
    string inspectionJson = JsonSerializer.Serialize(_inspection, _jsonOptions);
    Debug.WriteLine($"[Inspection Atualizada - CameraResult {result.Header.CameraId}]}}:\n{inspectionJson}");

    TryCompleteInspection();
  }

  public void Process(CameraImage image)
  {
    _inspection
        .GetCamera(image.CameraId)
        .SetImage(image.ImagePath);

    // Serializa o objeto Inspection inteiro para JSON
    string inspectionJson = JsonSerializer.Serialize(_inspection, _jsonOptions);
    Debug.WriteLine($"[Inspection Atualizada - CameraImage {image.CameraId}]}}:\n{inspectionJson}");

    TryCompleteInspection();
  }

  private void TryCompleteInspection()
  {

    if (!_inspection.IsCompleted)
    {
      return;
    }


    DomainInspection completedInspection = _inspection;

    _inspection = new DomainInspection();

    InspectionCompleted?.Invoke(completedInspection);
  }
}
