using Microsoft.Extensions.Logging;

namespace A2GestampApp.Infrastructure.Features.Images.Services;

public sealed class ImageFolderCleanupService
{
  private readonly ILogger<ImageFolderCleanupService> _logger;
  private readonly string _baseFolder;

  public ImageFolderCleanupService(ILogger<ImageFolderCleanupService> logger)
  {
    _logger = logger;

    _baseFolder = Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "Imagens");
  }

  public void CleanStartupFolders()
  {
    CleanFolder("VS1");
    CleanFolder("VS2");
    CleanFolder("VS3");
  }

  private void CleanFolder(string folderName)
  {
    var folder = Path.Combine(_baseFolder, folderName);

    try
    {
      if (!Directory.Exists(folder))
      {
        _logger.LogDebug(
            "Pasta de imagens não existe (será criada quando necessário): {Folder}",
            folder);

        return;
      }

      var files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
          .Where(f =>
              f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
              f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
              f.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
          .ToArray();

      foreach (var file in files)
      {
        try
        {
          File.Delete(file);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
          _logger.LogDebug(
              ex,
              "Não foi possível remover arquivo na inicialização (pode estar em uso): {File}",
              file);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(
              ex,
              "Falha ao remover arquivo na inicialização: {File}",
              file);
        }
      }

      if (files.Length > 0)
      {
        _logger.LogInformation(
            "Limpeza inicial: {Count} arquivo(s) removidos da pasta {Folder}",
            files.Length,
            folder);
      }
      else
      {
        _logger.LogDebug(
            "Limpeza inicial: pasta já vazia: {Folder}",
            folder);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Erro ao limpar pasta de imagens na inicialização: {Folder}",
          folder);
    }
  }
}
