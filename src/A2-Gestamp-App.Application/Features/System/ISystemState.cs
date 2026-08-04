namespace A2GestampApp.Application.Features.System;

public interface ISystemState
{
  public event Action? StateChanged;

  public MachineStatus MachineStatus { get; }

  public CommunicationStatus GetCamera1Status();

  public CommunicationStatus GetCamera2Status();

  public CommunicationStatus GetCamera3Status();

  public CommunicationStatus GetPlcStatus();

  public CommunicationStatus GetHikvisionStatus();

  public void SetCamera1Status(
      CommunicationStatus status);

  public void SetCamera2Status(
      CommunicationStatus status);

  public void SetCamera3Status(
      CommunicationStatus status);

  public void SetPlcStatus(
      CommunicationStatus status);

  public void SetHikvisionStatus(
      CommunicationStatus status);

  public void SetMachineStatus(
      MachineStatus status);

  public void NotifyStateChanged();
}
