namespace A2GestampApp.Application.Features.Keyence.Models;

public sealed record ToolResult(
    string Name,
    TimeSpan ExecutionTime,
    bool Approved)
{
  public double ExecutionTimeSeconds =>
      Math.Round(ExecutionTime.TotalSeconds, 2);
}
