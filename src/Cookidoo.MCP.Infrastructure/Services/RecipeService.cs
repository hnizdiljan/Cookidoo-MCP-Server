using Microsoft.Extensions.Logging;
using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Exceptions;
using Cookidoo.MCP.Core.Interfaces;

namespace Cookidoo.MCP.Infrastructure.Services;

/// <summary>
/// Implementace business logiky pro správu receptů
/// </summary>
public class RecipeService : IRecipeService
{
    private readonly ICookidooApiService _cookidooApiService;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(ICookidooApiService cookidooApiService, ILogger<RecipeService> logger)
    {
        _cookidooApiService = cookidooApiService;
        _logger = logger;
    }

    public async Task<Recipe> CreateRecipeAsync(string token, Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Vytváření receptu: {RecipeName}", recipe.Name);

            // Validace receptu
            var validationErrors = await ValidateRecipeAsync(recipe);
            if (validationErrors.Any())
            {
                throw new CookidooValidationException(validationErrors);
            }

            // Nastavení výchozích hodnot
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.UpdatedAt = DateTime.UtcNow;

            // Vytvoření v Cookidoo
            var createdRecipe = await _cookidooApiService.CreateRecipeAsync(token, recipe, cancellationToken);

            _logger.LogInformation("Recept úspěšně vytvořen s ID: {RecipeId}", createdRecipe.Id);
            return createdRecipe;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při vytváření receptu: {RecipeName}", recipe.Name);
            throw new CookidooException("Nepodařilo se vytvořit recept", ex);
        }
    }

    public async Task<Recipe> UpdateRecipeAsync(string token, string recipeId, Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Aktualizace receptu: {RecipeId}", recipeId);

            // Validace receptu
            var validationErrors = await ValidateRecipeAsync(recipe);
            if (validationErrors.Any())
            {
                throw new CookidooValidationException(validationErrors);
            }

            // Ověření existence receptu
            var existingRecipe = await _cookidooApiService.GetRecipeAsync(token, recipeId, cancellationToken);
            if (existingRecipe == null)
            {
                throw new CookidooNotFoundException($"Recept s ID {recipeId} nebyl nalezen");
            }

            // Aktualizace v Cookidoo
            var updatedRecipe = await _cookidooApiService.UpdateRecipeAsync(token, recipeId, recipe, cancellationToken);

            _logger.LogInformation("Recept úspěšně aktualizován: {RecipeId}", recipeId);
            return updatedRecipe;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při aktualizaci receptu: {RecipeId}", recipeId);
            throw new CookidooException("Nepodařilo se aktualizovat recept", ex);
        }
    }

    public async Task<Recipe?> GetRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání receptu: {RecipeId}", recipeId);

            var recipe = await _cookidooApiService.GetRecipeAsync(token, recipeId, cancellationToken);

            if (recipe != null)
            {
                _logger.LogInformation("Recept úspěšně načten: {RecipeId}", recipeId);
            }
            else
            {
                _logger.LogWarning("Recept nenalezen: {RecipeId}", recipeId);
            }

            return recipe;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání receptu: {RecipeId}", recipeId);
            throw new CookidooException("Nepodařilo se načíst recept", ex);
        }
    }

    public async Task<List<Recipe>> GetMyRecipesAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání vlastních receptů");

            var recipes = await _cookidooApiService.GetMyRecipesAsync(token, cancellationToken);

            _logger.LogInformation("Načteno {Count} vlastních receptů", recipes.Count);
            return recipes;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání vlastních receptů");
            throw new CookidooException("Nepodařilo se načíst vlastní recepty", ex);
        }
    }

    public async Task<bool> DeleteRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Mazání receptu: {RecipeId}", recipeId);

            // Ověření existence receptu
            var existingRecipe = await _cookidooApiService.GetRecipeAsync(token, recipeId, cancellationToken);
            if (existingRecipe == null)
            {
                _logger.LogWarning("Recept pro smazání nenalezen: {RecipeId}", recipeId);
                return false;
            }

            var result = await _cookidooApiService.DeleteRecipeAsync(token, recipeId, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Recept úspěšně smazán: {RecipeId}", recipeId);
            }

            return result;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při mazání receptu: {RecipeId}", recipeId);
            throw new CookidooException("Nepodařilo se smazat recept", ex);
        }
    }

    public async Task<List<string>> ValidateRecipeAsync(Recipe recipe)
    {
        var errors = new List<string>();

        // Základní validace
        if (string.IsNullOrWhiteSpace(recipe.Name))
        {
            errors.Add("Název receptu je povinný");
        }
        else if (recipe.Name.Length > 200)
        {
            errors.Add("Název receptu je příliš dlouhý (max 200 znaků)");
        }

        if (recipe.Description.Length > 2000)
        {
            errors.Add("Popis receptu je příliš dlouhý (max 2000 znaků)");
        }

        if (recipe.Notes.Length > 1000)
        {
            errors.Add("Poznámky k receptu jsou příliš dlouhé (max 1000 znaků)");
        }

        // Validace ingrediencí
        if (!recipe.Ingredients.Any())
        {
            errors.Add("Recept musí obsahovat alespoň jednu ingredienci");
        }
        else
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.Text))
                {
                    errors.Add("Ingredience musí mít vyplněný text");
                }
                else if (ingredient.Text.Length > 200)
                {
                    errors.Add($"Text ingredience je příliš dlouhý (max 200 znaků): {ingredient.Text}");
                }
            }
        }

        // Validace kroků
        if (!recipe.Steps.Any())
        {
            errors.Add("Recept musí obsahovat alespoň jeden krok přípravy");
        }
        else
        {
            foreach (var step in recipe.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.Text))
                {
                    errors.Add("Krok přípravy musí mít vyplněný text");
                }
                else if (step.Text.Length > 1000)
                {
                    errors.Add($"Text kroku je příliš dlouhý (max 1000 znaků): {step.Text.Substring(0, 50)}...");
                }
            }
        }

        // Validace časů
        if (recipe.PreparationTimeMinutes < 0 || recipe.PreparationTimeMinutes > 1440)
        {
            errors.Add("Čas přípravy musí být mezi 0 a 1440 minutami (24 hodin)");
        }

        if (recipe.CookingTimeMinutes < 0 || recipe.CookingTimeMinutes > 1440)
        {
            errors.Add("Čas vaření musí být mezi 0 a 1440 minutami (24 hodin)");
        }

        // Validace porcí
        if (recipe.Portions < 1 || recipe.Portions > 50)
        {
            errors.Add("Počet porcí musí být mezi 1 a 50");
        }

        return await Task.FromResult(errors);
    }

    public async Task<List<Recipe>> GetRecipesAsync(string token, int page, int pageSize, string? search = null, RecipeDifficulty? difficulty = null, List<string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání receptů - stránka: {Page}, velikost: {PageSize}, hledání: {Search}", page, pageSize, search);

            // Pro účely vývoje - vracíme vlastní recepty s filtrováním
            var allRecipes = await GetMyRecipesAsync(token, cancellationToken);

            // Aplikace filtrů
            var filteredRecipes = allRecipes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredRecipes = filteredRecipes.Where(r => 
                    r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (difficulty.HasValue)
            {
                filteredRecipes = filteredRecipes.Where(r => r.Difficulty == difficulty.Value);
            }

            if (tags != null && tags.Any())
            {
                filteredRecipes = filteredRecipes.Where(r => 
                    r.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
            }

            // Stránkování
            var pagedRecipes = filteredRecipes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Načteno {Count} receptů ze {Total}", pagedRecipes.Count, filteredRecipes.Count());
            return pagedRecipes;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání receptů");
            throw new CookidooException("Nepodařilo se načíst recepty", ex);
        }
    }

    public async Task<List<Recipe>> SearchRecipesAsync(string token, string query, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Vyhledávání receptů: {Query}, max výsledků: {MaxResults}", query, maxResults);

            // Pro účely vývoje - vyhledáváme ve vlastních receptech
            var allRecipes = await GetMyRecipesAsync(token, cancellationToken);

            var searchResults = allRecipes
                .Where(r => 
                    r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    r.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    r.Ingredients.Any(ing => ing.Text.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Take(maxResults)
                .ToList();

            _logger.LogInformation("Nalezeno {Count} receptů pro dotaz: {Query}", searchResults.Count, query);
            return searchResults;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při vyhledávání receptů");
            throw new CookidooException("Nepodařilo se vyhledat recepty", ex);
        }
    }
} 