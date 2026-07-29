namespace A2GestampApp.Application.Features.Keyence.Models;

public sealed record CameraInspectionResult(
    CameraHeader Header,
    IReadOnlyList<ToolResult> Tools);
