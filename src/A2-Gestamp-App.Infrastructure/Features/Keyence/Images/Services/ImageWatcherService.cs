namespace A2GestampApp.Infrastructure.Features.Images.Services;

public sealed class ImageWatcherService : IImageWatcherService
{
  private readonly List<FileSystemWatcher> _watchers = [];

  public event Action<CameraImage>? ImageReceived;

  public void Start()
  {
    CreateWatcher("VS1", 1);
    CreateWatcher("VS2", 2);
    CreateWatcher("VS3", 3);
  }

  private void CreateWatcher(string folderName, int cameraId)
  {
    string folder = Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "Imagens",
        folderName);

    Directory.CreateDirectory(folder);

    var watcher = new FileSystemWatcher(folder)
    {
      Filter = "*.*",
      IncludeSubdirectories = false,
      NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
      EnableRaisingEvents = true
    };

    watcher.Created += async (_, e) =>
    {
      // Usamos Task.Run para não travar a thread de eventos do FileSystemWatcher
      _ = Task.Run(() => OnFileCreatedSafeAsync(cameraId, e.FullPath));
    };

    _watchers.Add(watcher);
  }

  private async Task OnFileCreatedSafeAsync(int cameraId, string filePath)
  {
    if (Path.GetExtension(filePath).Equals(".svg", StringComparison.OrdinalIgnoreCase))
    {
      return;
    }

    Guid executionId = Guid.NewGuid();

    // Tenta abrir o arquivo repetidamente até que a câmera termine de gravá-lo (máximo 5 segundos)
    bool arquivoLiberado = false;
    for (int attempt = 1; attempt <= 50; attempt++)
    {
      try
      {
        // Tenta abrir com FileShare.Read para testar se a escrita acabou
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
          if (stream.Length > 0)
          {
            arquivoLiberado = true;
            break;
          }
        }
      }
      catch
      {
        // O arquivo ainda está sendo escrito pela câmera, aguarda um pouco
      }

      await Task.Delay(100);
    }

    if (!arquivoLiberado)
    {
      return;
    }

    var info = new FileInfo(filePath);

    ImageReceived?.Invoke(new CameraImage(cameraId, filePath));
  }
}
