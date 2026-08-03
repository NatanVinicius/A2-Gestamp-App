namespace A2GestampApp.Infrastructure.Hikvision;

internal static class HikvisionMultipartParser
{
  public static string? ExtractEventLog(string body)
  {
    const string marker = "name=\"event_log\"";

    var markerIndex = body.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

    if (markerIndex < 0)
    {
      return null;
    }

    var jsonStart = body.IndexOf("\r\n\r\n", markerIndex, StringComparison.Ordinal);

    if (jsonStart < 0)
    {
      return null;
    }

    jsonStart += 4;

    var boundaryIndex = body.IndexOf("\r\n--", jsonStart, StringComparison.Ordinal);

    if (boundaryIndex < 0)
    {
      return null;
    }

    return body[jsonStart..boundaryIndex].Trim();
  }
}
