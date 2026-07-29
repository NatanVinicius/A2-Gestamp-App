using System.Diagnostics;

using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Domain.Features.Inspection.Models;

namespace A2GestampApp.Infrastructure.Features.Images.Services;

public sealed class ImageTransferService : IImageTransferService
{
  private const int SlotCount = 3;

  private readonly string _wwwRoot;
  private readonly int[] _slots = { -1, -1, -1 };

  public ImageTransferService()
  {
    _wwwRoot = Path.Combine(
        AppContext.BaseDirectory,
        "wwwroot",
        "Assets",
        "Imagens");
  }

  public void Transfer(Inspection inspection)
  {
    TransferCamera(inspection.Camera1);
    TransferCamera(inspection.Camera2);
    TransferCamera(inspection.Camera3);
  }

  private void TransferCamera(CameraInspection camera)
  {
    if (string.IsNullOrWhiteSpace(camera.ImagePath))
    {
      return;
    }

    string destinationFolder =
        Path.Combine(_wwwRoot, $"VS{camera.CameraId}");

    Directory.CreateDirectory(destinationFolder);

    int cameraIndex = camera.CameraId - 1;

    _slots[cameraIndex] =
        (_slots[cameraIndex] + 1) % SlotCount;

    string fileName =
        $"slot{_slots[cameraIndex]}{Path.GetExtension(camera.ImagePath)}";

    string destinationFile =
        Path.Combine(destinationFolder, fileName);

    for (int attempt = 1; attempt <= 20; attempt++)
    {
      Debug.WriteLine($"Origem : {camera.ImagePath}");
      Debug.WriteLine($"Destino: {destinationFile}");

      try
      {
        File.Copy(
            camera.ImagePath,
            destinationFile,
            overwrite: true);

        // Exclui o arquivo original da pasta temporária após copiar com sucesso
        if (File.Exists(camera.ImagePath))
        {
          File.Delete(camera.ImagePath);
          Debug.WriteLine($"Original excluído: {camera.ImagePath}");
        }

        break;
      }
      catch (IOException ex)
      {
        Debug.WriteLine(ex.ToString());

        if (attempt == 20)
        {
          throw;
        }

        Thread.Sleep(100);
      }
    }

    camera.SetImage(
        Path.Combine(
            "Assets",
            "Imagens",
            $"VS{camera.CameraId}",
            fileName)
        .Replace('\\', '/'));
  }
}
