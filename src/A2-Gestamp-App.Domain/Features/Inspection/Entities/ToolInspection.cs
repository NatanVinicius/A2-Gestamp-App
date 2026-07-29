namespace A2_Gestamp_App.Domain.Features.Inspection.Entities;

public sealed class ToolInspection
{
  public string Name { get; init; } = string.Empty;

  public bool Approved { get; init; }

  public TimeSpan ExecutionTime { get; init; }
}
