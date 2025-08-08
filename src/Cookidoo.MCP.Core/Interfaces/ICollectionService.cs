using Cookidoo.MCP.Core.Entities;

namespace Cookidoo.MCP.Core.Interfaces;

/// <summary>
/// Interface pro business logiku správy kolekcí receptů
/// </summary>
public interface ICollectionService
{
    /// <summary>
    /// Vytvoření nové kolekce
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collection">Kolekce k vytvoření</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vytvořená kolekce</returns>
    Task<RecipeCollection> CreateCollectionAsync(string token, RecipeCollection collection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualizace existující kolekce
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="collection">Aktualizovaná kolekce</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aktualizovaná kolekce</returns>
    Task<RecipeCollection> UpdateCollectionAsync(string token, string collectionId, RecipeCollection collection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání detailu kolekce
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="includeRecipes">Zda načíst i detaily receptů</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detail kolekce nebo null pokud neexistuje</returns>
    Task<RecipeCollection?> GetCollectionAsync(string token, string collectionId, bool includeRecipes = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání seznamu vlastních kolekcí uživatele
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam vlastních kolekcí</returns>
    Task<List<RecipeCollection>> GetMyCollectionsAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Smazání kolekce (pokud je podporováno)
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

    /// <summary>
    /// Získání seznamu kolekcí s filtrováním a stránkováním
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="page">Číslo stránky</param>
    /// <param name="pageSize">Velikost stránky</param>
    /// <param name="search">Vyhledávací řetězec</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam kolekcí</returns>
    Task<List<RecipeCollection>> GetCollectionsAsync(string token, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Získání seznamu receptů v kolekci
    /// </summary>
    /// <param name="token">JWT token pro Cookidoo</param>
    /// <param name="collectionId">ID kolekce</param>
    /// <param name="page">Číslo stránky</param>
    /// <param name="pageSize">Velikost stránky</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam ID receptů</returns>
    Task<List<string>> GetCollectionRecipesAsync(string token, string collectionId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validace kolekce před odesláním do Cookidoo
    /// </summary>
    /// <param name="collection">Kolekce k validaci</param>
    /// <returns>Seznam chyb validace</returns>
    Task<List<string>> ValidateCollectionAsync(RecipeCollection collection);
} 