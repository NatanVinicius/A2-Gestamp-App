using A2GestampApp.Application.Features.Sku.Models;

namespace A2GestampApp.Application.Features.Sku;

public sealed class SkuService : ISkuService
{
  private readonly IPlcService _plcService;
  private readonly IKeyenceService _keyenceService;
  private readonly ISkuRecipeService _recipeService;

  private CancellationTokenSource? _pollingCancellation;

  private int? _table1ModelId;
  private int? _table2ModelId;

  public event Action? StateChanged;

  public string? Table1Model { get; private set; }

  public string? Table2Model { get; private set; }

  public int? CurrentTable { get; private set; }

  public SkuService(
      IPlcService plcService,
      IKeyenceService keyenceService,
      ISkuRecipeService recipeService)
  {
    _plcService = plcService;
    _keyenceService = keyenceService;
    _recipeService = recipeService;
  }

  public async Task ConfigureAsync(
    int? table1ModelId,
    int? table2ModelId)
  {
    Recipe? table1Recipe = null;
    Recipe? table2Recipe = null;

    if (table1ModelId.HasValue)
    {
      table1Recipe = await _recipeService.GetRecipeAsync(
          1,
          table1ModelId.Value);

      if (table1Recipe is null)
      {
        throw new InvalidOperationException(
            $"Receita {table1ModelId.Value} não encontrada para a Mesa 1.");
      }
    }

    if (table2ModelId.HasValue)
    {
      table2Recipe = await _recipeService.GetRecipeAsync(
          2,
          table2ModelId.Value);

      if (table2Recipe is null)
      {
        throw new InvalidOperationException(
            $"Receita {table2ModelId.Value} não encontrada para a Mesa 2.");
      }
    }

    // Cancela qualquer configuração/polling anterior.
    _pollingCancellation?.Cancel();
    _pollingCancellation?.Dispose();
    _pollingCancellation = null;

    _table1ModelId = table1ModelId;
    _table2ModelId = table2ModelId;

    Table1Model = table1Recipe?.Nome;
    Table2Model = table2Recipe?.Nome;

    CurrentTable = null;

    StateChanged?.Invoke();

    bool hasTable1Model =
        _table1ModelId.HasValue;

    bool hasTable2Model =
        _table2ModelId.HasValue;

    // Nenhum modelo.
    if (!hasTable1Model && !hasTable2Model)
    {
      await _keyenceService.SetModelAsync(0);

      return;
    }

    // Somente um modelo.
    if (!hasTable1Model || !hasTable2Model)
    {
      int modelId =
          _table1ModelId ??
          _table2ModelId!.Value;

      await _keyenceService.SetModelAsync(
          modelId);

      return;
    }

    // Dois modelos.
    await _plcService.SetWaitingAsync();

    _pollingCancellation =
        new CancellationTokenSource();

    _ = MonitorCurrentTableAsync(
        _pollingCancellation.Token);
  }

  public async Task ProcessInspectionAsync()
  {
    bool hasTable1Model =
        _table1ModelId.HasValue;

    bool hasTable2Model =
        _table2ModelId.HasValue;

    // Nenhum modelo.
    if (!hasTable1Model && !hasTable2Model)
    {
      await _keyenceService.SetModelAsync(0);

      return;
    }

    // Somente um modelo.
    if (!hasTable1Model || !hasTable2Model)
    {
      return;
    }

    // Dois modelos.
    // O PLC é a fonte de verdade da mesa.
    await SendModelFromPlcTableAsync();
  }

  private async Task SendModelFromPlcTableAsync()
  {
    int table =
        await _plcService.ReadCurrentTableAsync();

    CurrentTable = table;

    int modelId =
        table == 1
            ? _table1ModelId!.Value
            : _table2ModelId!.Value;

    await _keyenceService.SetModelAsync(
        modelId);
  }

  private async Task MonitorCurrentTableAsync(
    CancellationToken cancellationToken)
  {
    int? lastTable = null;

    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        ushort value =
            (ushort)await _plcService.ReadCurrentTableAsync();

        if (value is 1 or 2)
        {
          int table = value;

          if (lastTable != table)
          {
            lastTable = table;

            CurrentTable = table;

            StateChanged?.Invoke();

            int modelId =
                table == 1
                    ? _table1ModelId!.Value
                    : _table2ModelId!.Value;

            await _keyenceService.SetModelAsync(
                modelId);
          }
        }

        await Task.Delay(
            TimeSpan.FromMilliseconds(200),
            cancellationToken);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (Exception)
      {
        await Task.Delay(
            TimeSpan.FromMilliseconds(500),
            cancellationToken);
      }
    }
  }
}
