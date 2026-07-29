using System.Globalization;

using A2GestampApp.Application.Features.Keyence.Models;

namespace Infrastructure.Features.Keyence.Parsers;

public sealed class CameraMessageParser
{
  public CameraInspectionResult Parse(string rawMessage)
  {
    var lines = rawMessage
    .Split(@"\n", StringSplitOptions.RemoveEmptyEntries)
    .Select(line => line.Trim())
    .ToArray();

    if (lines.Length == 0)
    {
      throw new InvalidOperationException("The camera message is empty.");
    }

    var header = ParseHeader(lines[0]);

    var tools = new List<ToolResult>();

    foreach (var line in lines.Skip(1))
    {
      var tool = ParseTool(line);

      if (tool is not null)
      {
        tools.Add(tool);
      }
    }

    return new CameraInspectionResult(header, tools);
  }

  private CameraHeader ParseHeader(string line)
  {
    var values = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

    return new CameraHeader(
        CameraId: (int)double.Parse(values[0], CultureInfo.InvariantCulture),
        ExecutionTime: TimeSpan.FromMilliseconds(
            double.Parse(values[1], CultureInfo.InvariantCulture)),
        Approved: double.Parse(values[2], CultureInfo.InvariantCulture) == 1);
  }

  private ToolResult? ParseTool(string line)
  {
    var values = line.Split(',');

    // values[0] = ""
    // values[1] = Nome da ferramenta

    if (values.Length < 4 || values[1] == "-")
    {
      return null;
    }

    return new ToolResult(
        Name: values[1],
        ExecutionTime: TimeSpan.FromMilliseconds(
            double.Parse(values[2], CultureInfo.InvariantCulture)),
        Approved: double.Parse(values[3], CultureInfo.InvariantCulture) == 1);
  }
}
