namespace A2GestampApp.Application.Features.Keyence.Models;

public sealed record CameraHeader(
    int CameraId,
    TimeSpan ExecutionTime,
    bool Approved);
