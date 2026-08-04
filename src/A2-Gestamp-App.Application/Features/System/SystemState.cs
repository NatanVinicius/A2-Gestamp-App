namespace A2GestampApp.Application.Features.System;

public sealed class SystemState
    : ISystemState
{
  private readonly Dictionary<ExternalService, CommunicationStatus> _services;

  public event Action? StateChanged;

  public MachineStatus MachineStatus { get; private set; }

  public SystemState()
  {
    _services = Enum
        .GetValues<ExternalService>()
        .ToDictionary(
            service => service,
            _ => CommunicationStatus.Disconnected);

    MachineStatus = MachineStatus.Running;
  }

  public CommunicationStatus GetCamera1Status()
  {
    return _services[ExternalService.Camera1];
  }

  public CommunicationStatus GetCamera2Status()
  {
    return _services[ExternalService.Camera2];
  }

  public CommunicationStatus GetCamera3Status()
  {
    return _services[ExternalService.Camera3];
  }

  public CommunicationStatus GetPlcStatus()
  {
    return _services[ExternalService.Plc];
  }

  public CommunicationStatus GetHikvisionStatus()
  {
    return _services[ExternalService.HikvisionServer];
  }

  public void SetCamera1Status(
      CommunicationStatus status)
  {
    SetStatus(
        ExternalService.Camera1,
        status);
  }

  public void SetCamera2Status(
      CommunicationStatus status)
  {
    SetStatus(
        ExternalService.Camera2,
        status);
  }

  public void SetCamera3Status(
      CommunicationStatus status)
  {
    SetStatus(
        ExternalService.Camera3,
        status);
  }

  public void SetPlcStatus(
      CommunicationStatus status)
  {
    SetStatus(
        ExternalService.Plc,
        status);
  }

  public void SetHikvisionStatus(
      CommunicationStatus status)
  {
    SetStatus(
        ExternalService.HikvisionServer,
        status);
  }

  public void SetMachineStatus(
      MachineStatus status)
  {
    if (MachineStatus == status)
    {
      return;
    }

    MachineStatus = status;

    NotifyStateChanged();
  }

  public void NotifyStateChanged()
  {
    StateChanged?.Invoke();
  }

  private void SetStatus(
      ExternalService service,
      CommunicationStatus status)
  {
    if (_services[service] == status)
    {
      return;
    }

    _services[service] = status;

    NotifyStateChanged();
  }
}
