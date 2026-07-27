namespace Features.Inspection.Domain;

public sealed class ToolResult
{
  public required string Name { get; init; }

  public bool Passed { get; init; }

  public double ExecutionTime { get; init; }
}
