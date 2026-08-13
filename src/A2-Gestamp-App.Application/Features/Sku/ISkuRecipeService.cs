using A2GestampApp.Application.Features.Sku.Models;

namespace A2GestampApp.Application.Features.Sku;

public interface ISkuRecipeService
{
  public Task<IReadOnlyList<Recipe>> GetRecipesAsync(
      int table);

  public Task<Recipe?> GetRecipeAsync(
      int table,
      int recipeId);

  public Task<Recipe> AddRecipeAsync(
      int table,
      string name);

  public Task RemoveRecipeAsync(
      int table,
      int recipeId);
}
