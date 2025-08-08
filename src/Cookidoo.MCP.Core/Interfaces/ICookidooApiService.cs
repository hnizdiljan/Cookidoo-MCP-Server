using Cookidoo.MCP.Core.Entities;

namespace Cookidoo.MCP.Core.Interfaces;

/// <summary>
/// Interface pro komunikaci s Cookidoo API
/// </summary>
public interface ICookidooApiService
{
    /// <summary>
    /// Ověření platnosti autentizačního tokenu
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True pokud je token platný</returns>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání informací o uživateli
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Informace o uživateli</returns>
    Task<CookidooUser?> GetUserInfoAsync(string token, CancellationToken cancellationToken = default);

    // === RECEPTY ===

    /// <summary>
    /// Vytvoření nového receptu v Cookidoo
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipe">Recept k vytvoření</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vytvořený recept s ID z Cookidoo</returns>
    Task<Recipe> CreateRecipeAsync(string token, Recipe recipe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualizace existujícího receptu v Cookidoo
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="recipe">Aktualizovaný recept</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aktualizovaný recept</returns>
    Task<Recipe> UpdateRecipeAsync(string token, string recipeId, Recipe recipe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání detailu receptu z Cookidoo
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detail receptu</returns>
    Task<Recipe?> GetRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání seznamu vlastních receptů uživatele
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam vlastních receptů</returns>
    Task<List<Recipe>> GetMyRecipesAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Smazání receptu z Cookidoo (pokud API podporuje)
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True pokud se smazání podařilo</returns>
    Task<bool> DeleteRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default);

    // === KOLEKCE ===

    /// <summary>
    /// Vytvoření nové kolekce v Cookidoo
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collection">Kolekce k vytvoření</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vytvořená kolekce s ID z Cookidoo</returns>
    Task<RecipeCollection> CreateCollectionAsync(string token, RecipeCollection collection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualizace existující kolekce v Cookidoo
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="collection">Aktualizovaná kolekce</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aktualizovaná kolekce</returns>
    Task<RecipeCollection> UpdateCollectionAsync(string token, string collectionId, RecipeCollection collection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání detailu kolekce z Cookidoo
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="includeRecipes">Zda načíst i detaily receptů</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detail kolekce</returns>
    Task<RecipeCollection?> GetCollectionAsync(string token, string collectionId, bool includeRecipes = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání seznamu vlastních kolekcí uživatele
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam vlastních kolekcí</returns>
    Task<List<RecipeCollection>> GetMyCollectionsAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Smazání kolekce z Cookidoo (pokud API podporuje)
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True pokud se smazání podařilo</returns>
    Task<bool> DeleteCollectionAsync(string token, string collectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Přidání receptu do kolekce
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True pokud se přidání podařilo</returns>
    Task<bool> AddRecipeToCollectionAsync(string token, string collectionId, string recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Odebrání receptu z kolekce
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="recipeId">ID receptu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True pokud se odebrání podařilo</returns>
    Task<bool> RemoveRecipeFromCollectionAsync(string token, string collectionId, string recipeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Model uživatele Cookidoo
/// </summary>
public class CookidooUser
{
    /// <summary>
    /// ID uživatele
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Email uživatele
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Jméno uživatele
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Jazyk uživatele (např. "de-DE", "en-US")
    /// </summary>
    public string Language { get; set; } = "de-DE";
} 