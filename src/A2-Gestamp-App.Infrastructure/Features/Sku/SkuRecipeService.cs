using System.Text.Json;
using System.Text.Json.Serialization;

using A2GestampApp.Application.Features.Sku;
using A2GestampApp.Application.Features.Sku.Models;

namespace A2GestampApp.Infrastructure.Features.Sku;

public sealed class SkuRecipeService : ISkuRecipeService
{
  private const string FolderName = "Sku";
  private const string Table1FileName = "models-mesa-1.json";
  private const string Table2FileName = "models-mesa-2.json";

  private readonly string _directory;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public SkuRecipeService(string appDataDirectory)
  {
    _directory = Path.Combine(
        appDataDirectory,
        FolderName);
  }

  public async Task<IReadOnlyList<Recipe>> GetRecipesAsync(int table)
  {
    string path = GetFilePath(table);

    SkuConfiguration configuration =
        await LoadAsync(path);

    return configuration.Receitas;
  }

  public async Task<Recipe> AddRecipeAsync(
      int table,
      string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException(
          "O nome da receita não pode ser vazio.",
          nameof(name));
    }

    string path = GetFilePath(table);

    SkuConfiguration configuration =
        await LoadAsync(path);

    if (configuration.Receitas.Any(
        recipe => string.Equals(
            recipe.Nome,
            name,
            StringComparison.OrdinalIgnoreCase)))
    {
      throw new InvalidOperationException(
          "Já existe uma receita com esse nome.");
    }

    int nextId = configuration.Receitas.Count == 0
        ? 1
        : configuration.Receitas.Max(recipe => recipe.Id) + 1;

    Recipe recipe = new()
    {
      Id = nextId,
      Nome = name.Trim()
    };

    configuration.Receitas.Add(recipe);

    await SaveAsync(
        path,
        configuration);

    return recipe;
  }

  public async Task RemoveRecipeAsync(
      int table,
      int recipeId)
  {
    string path = GetFilePath(table);

    SkuConfiguration configuration =
        await LoadAsync(path);

    Recipe? recipe =
        configuration.Receitas.FirstOrDefault(
            item => item.Id == recipeId);

    if (recipe is null)
    {
      throw new InvalidOperationException(
          "Receita não encontrada.");
    }

    configuration.Receitas.Remove(recipe);

    await SaveAsync(
        path,
        configuration);
  }

  private async Task<SkuConfiguration> LoadAsync(
      string path)
  {
    Directory.CreateDirectory(_directory);

    if (!File.Exists(path))
    {
      SkuConfiguration skuConfiguration = new();

      if (path.EndsWith(
              Table1FileName,
              StringComparison.OrdinalIgnoreCase))
      {
        skuConfiguration.Receitas.AddRange(
        [
          new Recipe
      {
        Id = 1,
        Nome = "SKU-111"
      },
      new Recipe
      {
        Id = 2,
        Nome = "SKU-222"
      },
      new Recipe
      {
        Id = 3,
        Nome = "SKU-333"
      }
        ]);
      }
      else if (path.EndsWith(
                   Table2FileName,
                   StringComparison.OrdinalIgnoreCase))
      {
        skuConfiguration.Receitas.AddRange(
        [
          new Recipe
      {
        Id = 1,
        Nome = "SKU-444"
      },
      new Recipe
      {
        Id = 2,
        Nome = "SKU-555"
      },
      new Recipe
      {
        Id = 3,
        Nome = "SKU-666"
      }
        ]);
      }

      await SaveAsync(
          path,
          skuConfiguration);

      return skuConfiguration;
    }

    await using FileStream stream =
        File.OpenRead(path);

    SkuConfiguration? configuration =
        await JsonSerializer.DeserializeAsync<SkuConfiguration>(
            stream,
            JsonOptions);

    return configuration ?? new SkuConfiguration();
  }

  private async Task SaveAsync(
      string path,
      SkuConfiguration configuration)
  {
    Directory.CreateDirectory(_directory);

    await using FileStream stream =
        File.Create(path);

    await JsonSerializer.SerializeAsync(
        stream,
        configuration,
        JsonOptions);
  }

  private string GetFilePath(int table)
  {
    return table switch
    {
      1 => Path.Combine(
          _directory,
          Table1FileName),

      2 => Path.Combine(
          _directory,
          Table2FileName),

      _ => throw new ArgumentOutOfRangeException(
          nameof(table),
          "A mesa deve ser 1 ou 2.")
    };
  }

  public async Task<Recipe?> GetRecipeAsync(
    int table,
    int recipeId)
  {
    string path = GetFilePath(table);

    SkuConfiguration configuration =
        await LoadAsync(path);

    return configuration.Receitas
        .FirstOrDefault(
            recipe => recipe.Id == recipeId);
  }

  private sealed class SkuConfiguration
  {
    [JsonPropertyName("receitas")]
    public List<Recipe> Receitas { get; set; } = [];
  }
}
