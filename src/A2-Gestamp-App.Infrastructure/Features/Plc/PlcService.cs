using System.Net.Sockets;

using A2GestampApp.Application.Features.System;

using Microsoft.Extensions.Logging;

using NModbus;

namespace A2GestampApp.Infrastructure.Features.Plc;

public sealed class PlcService : IPlcService, IDisposable
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
      return;
    }

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

      _master = new ModbusFactory()
          .CreateMaster(_tcpClient);

      _systemState.SetPlcStatus(
          CommunicationStatus.Connected);

      _logger.LogInformation(
          "Connected to PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);

      await WriteAsync(
          PlcRegisters.SoftwareAlive,
          1);

      if (_heartbeatTask is null || _heartbeatTask.IsCompleted)
      {
        _heartbeatCancellationTokenSource = new();

        _heartbeatTask = HeartbeatAsync(
            _heartbeatCancellationTokenSource.Token);
      }
    }
    catch (OperationCanceledException)
    {
      _systemState.SetPlcStatus(
          CommunicationStatus.Disconnected);

      _logger.LogWarning(
          "Timeout connecting to PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);
    }
    catch (Exception ex)
    {
      _systemState.SetPlcStatus(
          CommunicationStatus.Disconnected);

      _logger.LogError(
          ex,
          "Error connecting to PLC ({IpAddress}:{Port}).",
          IpAddress,
          Port);
    }
  }

  public async Task DisconnectAsync()
  {
    try
    {
      if (_master is not null)
      {
        await WriteAsync(
            PlcRegisters.SoftwareAlive,
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
          "Error disconnecting PLC.");
    }

    _systemState.SetPlcStatus(
        CommunicationStatus.Disconnected);

    _logger.LogInformation(
        "PLC disconnected.");

    _tcpClient.Close();
  }

  public async Task WriteAsync(
      ushort register,
      ushort value)
  {
    if (_master is null)
    {
      return;
    }

    try
    {
      await _master.WriteSingleRegisterAsync(
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
          "Error writing PLC register {Register}.",
          register);

      throw;
    }
  }

  private async Task HeartbeatAsync(
    CancellationToken cancellationToken)
  {
    using PeriodicTimer timer =
        new(TimeSpan.FromSeconds(2));

    try
    {
      while (await timer.WaitForNextTickAsync(cancellationToken))
      {
        try
        {
          if (_master is null || !_tcpClient.Connected)
          {
            await ConnectAsync();
            continue;
          }

          await _master.WriteSingleRegisterAsync(
              SlaveId,
              PlcRegisters.SoftwareAlive,
              1);

          _systemState.SetPlcStatus(
              CommunicationStatus.Connected);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(
              ex,
              "PLC heartbeat failed. Reconnecting...");

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
            await ConnectAsync();
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
    }
  }

  public void Dispose()
  {
    _systemState.SetPlcStatus(
          CommunicationStatus.Disconnected);
    _tcpClient.Dispose();
  }
}
