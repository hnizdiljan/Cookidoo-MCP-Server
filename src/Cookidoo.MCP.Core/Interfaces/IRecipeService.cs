using Cookidoo.MCP.Core.Entities;

namespace Cookidoo.MCP.Core.Interfaces;

/// <summary>
/// Interface pro business logiku správy receptů
/// </summary>
public interface IRecipeService
{
    /// <summary>
    /// Vytvoření nového receptu
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipe">Recept k vytvoření</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vytvořený recept</returns>
    Task<Recipe> CreateRecipeAsync(string token, Recipe recipe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualizace existujícího receptu
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="recipe">Aktualizovaný recept</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aktualizovaný recept</returns>
    Task<Recipe> UpdateRecipeAsync(string token, string recipeId, Recipe recipe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání detailu receptu
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detail receptu nebo null pokud neexistuje</returns>
    Task<Recipe?> GetRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání seznamu vlastních receptů uživatele
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam vlastních receptů</returns>
    Task<List<Recipe>> GetMyRecipesAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Smazání receptu (pokud je podporováno)
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True pokud se smazání podařilo</returns>
    Task<bool> DeleteRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání seznamu receptů s filtrováním a stránkováním
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="page">Číslo stránky</param>
    /// <param name="pageSize">Velikost stránky</param>
    /// <param name="search">Vyhledávací řetězec</param>
    /// <param name="difficulty">Filtr podle obtížnosti</param>
    /// <param name="tags">Seznam tagů pro filtrování</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam receptů</returns>
    Task<List<Recipe>> GetRecipesAsync(string token, int page, int pageSize, string? search = null, RecipeDifficulty? difficulty = null, List<string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vyhledání receptů podle dotazu
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="query">Vyhledávací dotaz</param>
    /// <param name="maxResults">Maximální počet výsledků</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam nalezených receptů</returns>
    Task<List<Recipe>> SearchRecipesAsync(string token, string query, int maxResults = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validace receptu před odesláním do Cookidoo
    /// </summary>
    /// <param name="recipe">Recept k validaci</param>
    /// <returns>Seznam chyb validace</returns>
    Task<List<string>> ValidateRecipeAsync(Recipe recipe);
} 