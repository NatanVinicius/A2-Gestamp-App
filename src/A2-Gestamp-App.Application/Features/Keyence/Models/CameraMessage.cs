namespace A2GestampApp.Application.Features.Keyence.Models;

public sealed record CameraMessage(
    string CameraName,
    string RawMessage);
