namespace A2GestampApp.Domain.Features.Inspection.Models;

public sealed class ToolInspection
{
  public string Name { get; init; } = string.Empty;

  public bool Approved { get; init; }

  public TimeSpan ExecutionTime { get; init; }
}
