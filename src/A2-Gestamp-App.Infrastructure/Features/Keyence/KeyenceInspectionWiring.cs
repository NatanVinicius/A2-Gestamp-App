using A2GestampApp.Application.Features.Keyence;

using Features.Inspection.Domain;

namespace A2GestampApp.Infrastructure.Features.Keyence;

public sealed class KeyenceInspectionWiring
{
  public KeyenceInspectionWiring(
      IEnumerable<IKeyenceCamera> cameras,
      IInspectionService inspectionService)
  {
    foreach (var camera in cameras)
    {
      camera.InspectionReceived += (_, result) =>
      {
        if (inspectionService.CurrentInspection is null)
        {
          inspectionService.StartInspection();
        }

        inspectionService.AddCameraResult(result);
      };

      camera.ImageReceived += (_, image) =>
      {
        inspectionService.AddCameraImage(camera.CameraId, image);
      };
    }
  }
}
