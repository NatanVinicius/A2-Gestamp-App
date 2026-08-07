namespace A2GestampApp.Infrastructure.Hikvision;

internal static class HikvisionJpegExtractor
{
  private static readonly byte[] JpegStart = [0xFF, 0xD8];

  private static readonly byte[] JpegEnd = [0xFF, 0xD9];

  public static async Task<byte[]> ExtractAsync(
      Stream stream)
  {
    using var memory = new MemoryStream();

    await stream.CopyToAsync(memory);

    var buffer = memory.ToArray();

    var start = FindSequence(buffer, JpegStart);

    if (start < 0)
    {
      throw new InvalidOperationException(
          "Início da imagem JPEG não encontrado.");
    }

    var end = FindSequence(
        buffer,
        JpegEnd,
        start);

    if (end < 0)
    {
      throw new InvalidOperationException(
          "Fim da imagem JPEG não encontrado.");
    }

    end += 2;

    var image = new byte[end - start];

    Buffer.BlockCopy(
        buffer,
        start,
        image,
        0,
        image.Length);

    return image;
  }

  private static int FindSequence(
      byte[] source,
      byte[] sequence,
      int startIndex = 0)
  {
    for (var i = startIndex;
         i <= source.Length - sequence.Length;
         i++)
    {
      var found = true;

      for (var j = 0;
           j < sequence.Length;
           j++)
      {
        if (source[i + j] != sequence[j])
        {
          found = false;
          break;
        }
      }

      if (found)
      {
        return i;
      }
    }

    return -1;
  }
}
