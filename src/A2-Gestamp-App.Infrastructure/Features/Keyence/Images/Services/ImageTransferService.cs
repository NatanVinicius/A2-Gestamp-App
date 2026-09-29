using A2_Gestamp_App.Domain.Features.Inspection.Entities;

using A2GestampApp.Application.Features.Images.Services;
using A2GestampApp.Domain.Features.Inspection.Entities;
using A2GestampApp.Domain.Features.Inspection.Enums;
using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Features.Images.Services;

public sealed class ImageTransferService : IImageTransferService
{
  private readonly string _wwwRoot;
  private readonly ILogger<ImageTransferService> _logger;

  public ImageTransferService(ILogger<ImageTransferService> logger)
  {
    _logger = logger;

    _wwwRoot = Path.Combine(
        AppContext.BaseDirectory,
        "wwwroot",
        "Assets",
        "Imagens");
  }

  public void Transfer(Inspection inspection)
  {
    string? rejectedFolder = null;
    string? rejectedFolderName = null;

    if (inspection.FinalJudgement == InspectionResult.Reprovada)
    {
      rejectedFolderName =
          DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

      rejectedFolder = Path.Combine(
          AppContext.BaseDirectory,
          "Assets",
          "Rejeitos",
          rejectedFolderName);

      Directory.CreateDirectory(rejectedFolder);

      inspection.SetRejectFolder(rejectedFolderName);
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

    // Salva evidências originais (usa cópia simples com retry, mantém nomes originais)
    if (rejectedFolder is not null)
    {
      var rejectedImagePath = Path.Combine(rejectedFolder, Path.GetFileName(sourceImage));
      var rejectedOverlayPath = Path.Combine(rejectedFolder, Path.GetFileName(sourceOverlay));

      CopyFileWithRetry(sourceImage, rejectedImagePath);
      CopyFileWithRetry(sourceOverlay, rejectedOverlayPath);
    }

    var destinationFolder =
        Path.Combine(_wwwRoot, $"VS{camera.CameraId}");

    Directory.CreateDirectory(destinationFolder);

    // Guarda referências anteriores (para tentativa de limpeza)
    var previousImagePath = camera.ImagePath;
    var previousOverlayPath = camera.OverlayPath;

    // Gera nomes únicos por versão para evitar substituição de arquivo em uso
    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
    var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);

    var imageName = $"camera{camera.CameraId}_{timestamp}_{suffix}.jpg";
    var overlayName = $"camera{camera.CameraId}_{timestamp}_{suffix}.svg";

    var destinationImage =
        Path.Combine(destinationFolder, imageName);

    var destinationOverlay =
        Path.Combine(destinationFolder, overlayName);

    // Escreve diretamente com CreateNew (sem substituir arquivo existente)
    using (var src = new FileStream(sourceImage, FileMode.Open, FileAccess.Read, FileShare.Read))
    using (var dst = new FileStream(destinationImage, FileMode.CreateNew, FileAccess.Write, FileShare.None))
    {
      src.CopyTo(dst);
      dst.Flush(true);
    }

    using (var src = new FileStream(sourceOverlay, FileMode.Open, FileAccess.Read, FileShare.Read))
    using (var dst = new FileStream(destinationOverlay, FileMode.CreateNew, FileAccess.Write, FileShare.None))
    {
      src.CopyTo(dst);
      dst.Flush(true);
    }

    // Deleta fontes (arquivos da pasta Assets\Imagens) — comportamento mantido
    File.Delete(sourceImage);
    File.Delete(sourceOverlay);

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

    // Agendar exclusão best-effort dos arquivos anteriores (se houver)
    TryDeletePrevious(previousImagePath, previousOverlayPath);
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

  private void CopyFileWithRetry(string source, string destination)
  {
    // Garante que o diretório destino existe
    var destDir = Path.GetDirectoryName(destination);
    if (!string.IsNullOrEmpty(destDir))
    {
      Directory.CreateDirectory(destDir);
    }

    for (int attempt = 1; attempt <= 20; attempt++)
    {
      try
      {
        using (var src = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var dst = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
          src.CopyTo(dst);
          dst.Flush(true);
        }

        return;
      }
      catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
      {
        if (attempt == 20)
        {
          _logger?.LogError(ex, "Falha ao copiar arquivo após {Attempts} tentativas. Source: {Source} Dest: {Destination}", attempt, source, destination);
          throw;
        }

        _logger?.LogWarning(ex, "Falha ao copiar arquivo (tentativa {Attempt}/20). Source: {Source} Dest: {Destination}", attempt, source, destination);

        Thread.Sleep(100);
      }
    }
  }

  private void TryDeletePrevious(string? previousImagePath, string? previousOverlayPath)
  {
    if (string.IsNullOrWhiteSpace(previousImagePath) && string.IsNullOrWhiteSpace(previousOverlayPath))
    {
      return;
    }

    _ = Task.Run(() =>
    {
      foreach (var path in new[] { previousImagePath, previousOverlayPath })
      {
        if (string.IsNullOrWhiteSpace(path))
        {
          continue;
        }

        try
        {
          if (File.Exists(path))
          {
            File.Delete(path);
            _logger?.LogDebug("Arquivo anterior removido: {Path}", path);
          }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
          _logger?.LogDebug(ex, "Não foi possível remover arquivo anterior (pode estar em uso): {Path}", path);
        }
        catch (Exception ex)
        {
          _logger?.LogWarning(ex, "Falha inesperada ao tentar remover arquivo anterior: {Path}", path);
        }
      }
    });
  }
}
