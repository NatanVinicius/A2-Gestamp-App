using A2GestampApp.Application.Features.Sku;
using A2GestampApp.Application.Features.Sku.Models;

using Microsoft.AspNetCore.Components;

namespace A2GestampApp.Client.Components.Shared.SkuConfigurationDialog;

public partial class SkuConfigurationDialog : ComponentBase
{
  [Inject]
  private ISkuRecipeService RecipeService { get; set; } = default!;

  [Inject]
  private ISkuService SkuService { get; set; } = default!;

  [Parameter]
  public bool IsOpen { get; set; }

  [Parameter]
  public EventCallback<bool> IsOpenChanged { get; set; }

  private IReadOnlyList<Recipe> Table1Recipes =
      Array.Empty<Recipe>();

  private IReadOnlyList<Recipe> Table2Recipes =
      Array.Empty<Recipe>();

  private int? Table1ModelId;

  private int? Table2ModelId;

  private bool IsSaving;

  private string? ErrorMessage;

  protected override async Task OnParametersSetAsync()
  {
    if (!IsOpen)
    {
      return;
    }

    await LoadRecipesAsync();

    Table1ModelId = GetSelectedModelId(
        SkuService.Table1Model,
        Table1Recipes);

    Table2ModelId = GetSelectedModelId(
        SkuService.Table2Model,
        Table2Recipes);

    ErrorMessage = null;
  }

  private async Task LoadRecipesAsync()
  {
    Table1Recipes =
        await RecipeService.GetRecipesAsync(1);

    Table2Recipes =
        await RecipeService.GetRecipesAsync(2);
  }

  private static int? GetSelectedModelId(
      string? modelName,
      IReadOnlyList<Recipe> recipes)
  {
    if (string.IsNullOrWhiteSpace(modelName))
    {
      return null;
    }

    return recipes
        .FirstOrDefault(recipe =>
            recipe.Nome == modelName)
        ?.Id;
  }

  private async Task Confirm()
  {
    if (IsSaving)
    {
      return;
    }

    try
    {
      IsSaving = true;
      ErrorMessage = null;

      await SkuService.ConfigureAsync(
          Table1ModelId,
          Table2ModelId);

      await Close();
    }
    catch (Exception ex)
    {
      ErrorMessage = ex.Message;
    }
    finally
    {
      IsSaving = false;
    }
  }

  private async Task Close()
  {
    IsOpen = false;

    await IsOpenChanged.InvokeAsync(false);
  }
}
