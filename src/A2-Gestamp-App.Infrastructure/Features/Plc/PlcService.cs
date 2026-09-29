using System.Net.Sockets;

using A2GestampApp.Application.Features.System;

using Microsoft.Extensions.Logging;

using NModbus;

namespace A2GestampApp.Infrastructure.Features.Plc;

public sealed class PlcService : IPlcService, IAsyncDisposable
{
  private const string IpAddress = "192.168.70.20";

  private const int Port = 502;

  private const byte SlaveId = 1;

  private TcpClient _tcpClient = new();

  private readonly ISystemState _systemState;

  private readonly ILogger<PlcService> _logger;

  private IModbusMaster? _master;

  private CancellationTokenSource? _heartbeatCancellationTokenSource;

  private Task? _heartbeatTask;

  public PlcService(
    ISystemState systemState,
    ILogger<PlcService> logger)
  {
    _systemState = systemState;
    _logger = logger;
  }

  public async Task ConnectAsync()
  {
    if (_tcpClient.Connected)
    {
      _logger.LogDebug(
          "[PLC] Already connected to PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);

      return;
    }

    _logger.LogDebug(
        "[PLC] Connecting to PLC ({IpAddress}:{Port}).",
        IpAddress,
        Port);

    _tcpClient.Dispose();
    _tcpClient = new TcpClient();

    try
    {
      using CancellationTokenSource cancellationTokenSource =
          new(TimeSpan.FromSeconds(3));

      await _tcpClient.ConnectAsync(
          IpAddress,
          Port,
          cancellationTokenSource.Token);

      _logger.LogDebug(
          "[PLC] TCP connection established with PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);

      _master = new ModbusFactory()
          .CreateMaster(_tcpClient);

      _systemState.SetPlcStatus(
          CommunicationStatus.Connected);

      _logger.LogDebug(
          "[PLC] Connected to PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);

      await WriteAsync(
          PlcRegisters.SoftwareAlive,
          1);

      await WriteAsync(
         PlcRegisters.SoftwareAliveConfirm,
         1);

      if (_heartbeatTask is null || _heartbeatTask.IsCompleted)
      {
        _heartbeatCancellationTokenSource = new();

        _heartbeatTask = HeartbeatAsync(
            _heartbeatCancellationTokenSource.Token);

        _logger.LogDebug(
            "[PLC] Heartbeat started.");
      }
    }
    catch (OperationCanceledException)
    {
      _systemState.SetPlcStatus(
          CommunicationStatus.Disconnected);

      _logger.LogWarning(
          "[PLC] Timeout connecting to PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);
    }
    catch (Exception ex)
    {
      _systemState.SetPlcStatus(
          CommunicationStatus.Disconnected);

      _logger.LogError(
          ex,
          "[PLC] Error connecting to PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);
    }
  }

  public async Task DisconnectAsync()
  {
    _logger.LogDebug(
        "[PLC] Disconnecting from PLC.");

    try
    {
      if (_master is not null)
      {
        _logger.LogDebug(
            "[PLC] Writing SoftwareAlive = 0 before disconnect.");

        await WriteAsync(
            PlcRegisters.SoftwareAlive,
            0);

        await _master.WriteSingleRegisterAsync(
              SlaveId,
              PlcRegisters.SoftwareAliveConfirm,
              0);

        _heartbeatCancellationTokenSource?.Cancel();

        if (_heartbeatTask is not null)
        {
          try
          {
            await _heartbeatTask;
          }
          catch
          {
          }
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "[PLC] Error disconnecting PLC.");
    }

    _systemState.SetPlcStatus(
        CommunicationStatus.Disconnected);

    _logger.LogDebug(
        "[PLC] PLC disconnected.");

    _tcpClient.Close();
  }

  public async Task WriteAsync(
      ushort register,
      ushort value)
  {
    if (_master is null)
    {
      _logger.LogWarning(
          "[PLC] Write ignored because PLC master is not initialized. Register: {Register}, Value: {Value}",
          register,
          value);

      return;
    }

    _logger.LogDebug(
        "[PLC] Writing register. SlaveId: {SlaveId}, Register: {Register}, Value: {Value}",
        SlaveId,
        register,
        value);

    try
    {
      await _master.WriteSingleRegisterAsync(
          SlaveId,
          register,
          value);

      _logger.LogDebug(
          "[PLC] Register written successfully. SlaveId: {SlaveId}, Register: {Register}, Value: {Value}",
          SlaveId,
          register,
          value);
    }
    catch (Exception ex)
    {
      _systemState.SetPlcStatus(
          CommunicationStatus.Disconnected);

      _logger.LogError(
          ex,
          "[PLC] Error writing register. SlaveId: {SlaveId}, Register: {Register}, Value: {Value}",
          SlaveId,
          register,
          value);

      throw;
    }
  }

  public async Task SetWaitingAsync()
  {
    await WriteAsync(
        PlcRegisters.Waiting,
        1);
  }

  public async Task<ushort> ReadAsync(
    ushort register)
  {
    if (_master is null)
    {
      _logger.LogWarning(
          "[PLC] Read ignored because PLC master is not initialized. Register: {Register}",
          register);

      throw new InvalidOperationException(
          "PLC master is not initialized.");
    }

    _logger.LogDebug(
        "[PLC] Reading register. SlaveId: {SlaveId}, Register: {Register}",
        SlaveId,
        register);

    try
    {
      ushort[] values =
          await _master.ReadHoldingRegistersAsync(
              SlaveId,
              register,
              1);

      ushort value = values[0];

      _logger.LogDebug(
          "[PLC] Register read successfully. SlaveId: {SlaveId}, Register: {Register}, Value: {Value}",
          SlaveId,
          register,
          value);

      return value;
    }
    catch (Exception ex)
    {
      _systemState.SetPlcStatus(
          CommunicationStatus.Disconnected);

      _logger.LogError(
          ex,
          "[PLC] Error reading register. SlaveId: {SlaveId}, Register: {Register}",
          SlaveId,
          register);

      throw;
    }
  }

  public async Task<int> ReadCurrentTableAsync()
  {
    ushort value =
        await ReadAsync(
            PlcRegisters.CurrentTable);

    return value switch
    {
      1 => 1,
      2 => 2,

      _ => throw new InvalidOperationException(
          $"PLC retornou uma mesa inválida no D6: {value}.")
    };
  }

  private async Task HeartbeatAsync(
      CancellationToken cancellationToken)
  {
    using PeriodicTimer timer =
        new(TimeSpan.FromSeconds(2));

    _logger.LogInformation(
        "[PLC] Heartbeat loop running.");

    try
    {
      while (await timer.WaitForNextTickAsync(cancellationToken))
      {
        try
        {
          if (_master is null || !_tcpClient.Connected)
          {
            _logger.LogWarning(
                "[PLC] Heartbeat detected disconnected PLC. Reconnecting...");

            await ConnectAsync();

            continue;
          }

          await _master.WriteSingleRegisterAsync(
              SlaveId,
              PlcRegisters.SoftwareAlive,
              1);

          await _master.WriteSingleRegisterAsync(
              SlaveId,
              PlcRegisters.SoftwareAliveConfirm,
              1);

          _systemState.SetPlcStatus(
              CommunicationStatus.Connected);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(
              ex,
              "[PLC] Heartbeat failed. Reconnecting...");

          _systemState.SetPlcStatus(
              CommunicationStatus.Disconnected);

          _master = null;

          try
          {
            _tcpClient.Dispose();
          }
          catch
          {
          }

          _tcpClient = new TcpClient();

          try
          {
            _logger.LogInformation(
                "[PLC] Attempting PLC reconnection after heartbeat failure.");

            await ConnectAsync();

            _logger.LogInformation(
                "[PLC] PLC reconnection attempt completed.");
          }
          catch
          {
            // A próxima iteração tentará novamente.
          }
        }
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation(
          "[PLC] Heartbeat cancelled.");
    }
  }

  public async ValueTask DisposeAsync()
  {
    _logger.LogInformation(
        "[PLC] Disposing PLC service.");

    _systemState.SetPlcStatus(
        CommunicationStatus.Disconnected);
    await WriteAsync(
            PlcRegisters.SoftwareAlive,
            0);

    await WriteAsync(
         PlcRegisters.SoftwareAliveConfirm,
         0);

    _tcpClient.Dispose();

    _logger.LogInformation(
        "[PLC] PLC service disposed.");
  }
}
