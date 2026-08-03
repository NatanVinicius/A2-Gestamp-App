using System.Diagnostics;

using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.Inspection.Enums;

namespace A2GestampApp.Infrastructure.Features.Images.Services;

public sealed class ImageTransferService : IImageTransferService
{
  private readonly string _wwwRoot;

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

    Debug.WriteLine($"TRANSFER {DateTime.Now:HH:mm:ss.fff}");


    string? rejectedFolder = null;

    if (inspection.Result == InspectionResult.Reprovada)
    {
      rejectedFolder = Path.Combine(
          AppContext.BaseDirectory,
          "Assets",
          "Rejeitos",
          DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

      Directory.CreateDirectory(rejectedFolder);
    }

    TransferCamera(inspection.Camera1, rejectedFolder);
    TransferCamera(inspection.Camera2, rejectedFolder);
    TransferCamera(inspection.Camera3, rejectedFolder);
  }

  private void TransferCamera(
      CameraInspection camera,
      string? rejectedFolder)
  {


    if (string.IsNullOrWhiteSpace(camera.ImagePath))
    {
      return;
    }

    var sourceImage = camera.ImagePath;
    var sourceOverlay = Path.ChangeExtension(sourceImage, ".svg");



    Debug.WriteLine($"CAM {camera.CameraId}");
    Debug.WriteLine(sourceImage);
    Debug.WriteLine(File.Exists(sourceImage));
    Debug.WriteLine(File.Exists(sourceOverlay));


    WaitFileAvailable(sourceImage);
    WaitFileAvailable(sourceOverlay);


    // Aguarda o SVG e jpg existir
    for (int attempt = 1; attempt <= 20; attempt++)
    {
      if (File.Exists(sourceImage) &&
          File.Exists(sourceOverlay))
      {
        break;
      }

      if (attempt == 20)
      {
        throw new FileNotFoundException(
            $"Arquivos não encontrados:\n{sourceImage}\n{sourceOverlay}");
      }

      Thread.Sleep(100);
    }

    // Salva evidências originais
    if (rejectedFolder is not null)
    {
      File.Copy(
          sourceImage,
          Path.Combine(rejectedFolder, Path.GetFileName(sourceImage)),
          true);

      File.Copy(
          sourceOverlay,
          Path.Combine(rejectedFolder, Path.GetFileName(sourceOverlay)),
          true);
    }

    var destinationFolder =
        Path.Combine(_wwwRoot, $"VS{camera.CameraId}");

    Directory.CreateDirectory(destinationFolder);

    var imageName = $"camera{camera.CameraId}.jpg";
    var overlayName = $"camera{camera.CameraId}.svg";

    var destinationImage =
        Path.Combine(destinationFolder, imageName);

    var destinationOverlay =
        Path.Combine(destinationFolder, overlayName);

    for (int attempt = 1; attempt <= 20; attempt++)
    {
      try
      {
        File.Copy(sourceImage, destinationImage, true);
        File.Copy(sourceOverlay, destinationOverlay, true);

        File.Delete(sourceImage);
        File.Delete(sourceOverlay);

        break;
      }
      catch (IOException)
      {
        if (attempt == 20)
        {
          throw;
        }

        Thread.Sleep(100);
      }
    }

    if (!File.Exists(destinationImage))
    {
      throw new FileNotFoundException(destinationImage);
    }

    if (!File.Exists(destinationOverlay))
    {
      throw new FileNotFoundException(destinationOverlay);
    }

    camera.SetImage(
        Path.Combine(
            "Assets",
            "Imagens",
            $"VS{camera.CameraId}",
            imageName)
        .Replace('\\', '/'));

    camera.SetOverlay(
        Path.Combine(
            "Assets",
            "Imagens",
            $"VS{camera.CameraId}",
            overlayName)
        .Replace('\\', '/'));

    Debug.WriteLine($"END CAM {camera.CameraId}");
  }

  private static void WaitFileAvailable(string path)
  {
    for (int attempt = 1; attempt <= 20; attempt++)
    {
      try
      {
        using var stream = File.Open(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None);

        return;
      }
      catch (IOException)
      {
        if (attempt == 20)
        {
          throw;
        }

        Thread.Sleep(100);
      }
    }
  }
}
