namespace A2GestampApp.Infrastructure.Features.Images.Configuration;

public sealed class ImageFolderOptions
{
  public string Name { get; init; } = string.Empty;

  public int CameraId { get; init; }

  public string SourceFolder { get; init; } = string.Empty;

  public string DestinationFolder { get; init; } = string.Empty;
}
