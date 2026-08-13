namespace A2GestampApp.Application.Features.Sku;

public interface ISkuService
{
  public string? Table1Model { get; }

  public string? Table2Model { get; }

  public int? CurrentTable { get; }

  public event Action? StateChanged;

  public Task ConfigureAsync(
      int? table1ModelId,
      int? table2ModelId);

  public Task ProcessInspectionAsync();
}
