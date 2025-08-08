using Microsoft.Extensions.Logging;
using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Exceptions;
using Cookidoo.MCP.Core.Interfaces;

namespace Cookidoo.MCP.Infrastructure.Services;

/// <summary>
/// Implementace business logiky pro správu kolekcí receptů
/// </summary>
public class CollectionService : ICollectionService
{
    private readonly ICookidooApiService _cookidooApiService;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(ICookidooApiService cookidooApiService, ILogger<CollectionService> logger)
    {
        _cookidooApiService = cookidooApiService;
        _logger = logger;
    }

    public async Task<RecipeCollection> CreateCollectionAsync(string token, RecipeCollection collection, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Vytváření kolekce: {CollectionName}", collection.Name);

            // Validace kolekce
            var validationErrors = await ValidateCollectionAsync(collection);
            if (validationErrors.Any())
            {
                throw new CookidooValidationException(validationErrors);
            }

            // Nastavení výchozích hodnot
            collection.CreatedAt = DateTime.UtcNow;
            collection.UpdatedAt = DateTime.UtcNow;

            // Vytvoření v Cookidoo
            var createdCollection = await _cookidooApiService.CreateCollectionAsync(token, collection, cancellationToken);

            _logger.LogInformation("Kolekce úspěšně vytvořena s ID: {CollectionId}", createdCollection.Id);
            return createdCollection;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při vytváření kolekce: {CollectionName}", collection.Name);
            throw new CookidooException("Nepodařilo se vytvořit kolekci", ex);
        }
    }

    public async Task<RecipeCollection> UpdateCollectionAsync(string token, string collectionId, RecipeCollection collection, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Aktualizace kolekce: {CollectionId}", collectionId);

            // Validace kolekce
            var validationErrors = await ValidateCollectionAsync(collection);
            if (validationErrors.Any())
            {
                throw new CookidooValidationException(validationErrors);
            }

            // Ověření existence kolekce
            var existingCollection = await _cookidooApiService.GetCollectionAsync(token, collectionId, false, cancellationToken);
            if (existingCollection == null)
            {
                throw new CookidooNotFoundException($"Kolekce s ID {collectionId} nebyla nalezena");
            }

            // Aktualizace v Cookidoo
            var updatedCollection = await _cookidooApiService.UpdateCollectionAsync(token, collectionId, collection, cancellationToken);

            _logger.LogInformation("Kolekce úspěšně aktualizována: {CollectionId}", collectionId);
            return updatedCollection;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při aktualizaci kolekce: {CollectionId}", collectionId);
            throw new CookidooException("Nepodařilo se aktualizovat kolekci", ex);
        }
    }

    public async Task<RecipeCollection?> GetCollectionAsync(string token, string collectionId, bool includeRecipes = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání kolekce: {CollectionId}", collectionId);

            var collection = await _cookidooApiService.GetCollectionAsync(token, collectionId, includeRecipes, cancellationToken);

            if (collection != null)
            {
                _logger.LogInformation("Kolekce úspěšně načtena: {CollectionId}", collectionId);
            }
            else
            {
                _logger.LogWarning("Kolekce nenalezena: {CollectionId}", collectionId);
            }

            return collection;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání kolekce: {CollectionId}", collectionId);
            throw new CookidooException("Nepodařilo se načíst kolekci", ex);
        }
    }

    public async Task<List<RecipeCollection>> GetMyCollectionsAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání vlastních kolekcí");

            var collections = await _cookidooApiService.GetMyCollectionsAsync(token, cancellationToken);

            _logger.LogInformation("Načteno {Count} vlastních kolekcí", collections.Count);
            return collections;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání vlastních kolekcí");
            throw new CookidooException("Nepodařilo se načíst vlastní kolekce", ex);
        }
    }

    public async Task<bool> DeleteCollectionAsync(string token, string collectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Mazání kolekce: {CollectionId}", collectionId);

            // Ověření existence kolekce
            var existingCollection = await _cookidooApiService.GetCollectionAsync(token, collectionId, false, cancellationToken);
            if (existingCollection == null)
            {
                _logger.LogWarning("Kolekce pro smazání nenalezena: {CollectionId}", collectionId);
                return false;
            }

            var result = await _cookidooApiService.DeleteCollectionAsync(token, collectionId, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Kolekce úspěšně smazána: {CollectionId}", collectionId);
            }

            return result;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při mazání kolekce: {CollectionId}", collectionId);
            throw new CookidooException("Nepodařilo se smazat kolekci", ex);
        }
    }

    public async Task<bool> AddRecipeToCollectionAsync(string token, string collectionId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Přidávání receptu {RecipeId} do kolekce {CollectionId}", recipeId, collectionId);

            // Ověření existence kolekce
            var collection = await _cookidooApiService.GetCollectionAsync(token, collectionId, false, cancellationToken);
            if (collection == null)
            {
                throw new CookidooNotFoundException($"Kolekce s ID {collectionId} nebyla nalezena");
            }

            // Ověření existence receptu
            var recipe = await _cookidooApiService.GetRecipeAsync(token, recipeId, cancellationToken);
            if (recipe == null)
            {
                throw new CookidooNotFoundException($"Recept s ID {recipeId} nebyl nalezen");
            }

            // Kontrola, zda už recept v kolekci není
            if (collection.RecipeIds.Contains(recipeId))
            {
                _logger.LogWarning("Recept {RecipeId} už je v kolekci {CollectionId}", recipeId, collectionId);
                return true; // Považujeme za úspěch
            }

            var result = await _cookidooApiService.AddRecipeToCollectionAsync(token, collectionId, recipeId, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Recept úspěšně přidán do kolekce");
            }

            return result;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při přidávání receptu do kolekce");
            throw new CookidooException("Nepodařilo se přidat recept do kolekce", ex);
        }
    }

    public async Task<bool> RemoveRecipeFromCollectionAsync(string token, string collectionId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Odebírání receptu {RecipeId} z kolekce {CollectionId}", recipeId, collectionId);

            // Ověření existence kolekce
            var collection = await _cookidooApiService.GetCollectionAsync(token, collectionId, false, cancellationToken);
            if (collection == null)
            {
                throw new CookidooNotFoundException($"Kolekce s ID {collectionId} nebyla nalezena");
            }

            // Kontrola, zda recept v kolekci je
            if (!collection.RecipeIds.Contains(recipeId))
            {
                _logger.LogWarning("Recept {RecipeId} není v kolekci {CollectionId}", recipeId, collectionId);
                return true; // Považujeme za úspěch
            }

            var result = await _cookidooApiService.RemoveRecipeFromCollectionAsync(token, collectionId, recipeId, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Recept úspěšně odebrán z kolekce");
            }

            return result;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při odebírání receptu z kolekce");
            throw new CookidooException("Nepodařilo se odebrat recept z kolekce", ex);
        }
    }

    public async Task<List<string>> ValidateCollectionAsync(RecipeCollection collection)
    {
        var errors = new List<string>();

        // Základní validace
        if (string.IsNullOrWhiteSpace(collection.Name))
        {
            errors.Add("Název kolekce je povinný");
        }
        else if (collection.Name.Length > 200)
        {
            errors.Add("Název kolekce je příliš dlouhý (max 200 znaků)");
        }

        if (collection.Description.Length > 1000)
        {
            errors.Add("Popis kolekce je příliš dlouhý (max 1000 znaků)");
        }

        // Validace tagů
        foreach (var tag in collection.Tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                errors.Add("Tag nesmí být prázdný");
            }
            else if (tag.Length > 50)
            {
                errors.Add($"Tag je příliš dlouhý (max 50 znaků): {tag}");
            }
        }

        return await Task.FromResult(errors);
    }

    public async Task<List<RecipeCollection>> GetCollectionsAsync(string token, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání kolekcí - stránka: {Page}, velikost: {PageSize}, hledání: {Search}", page, pageSize, search);

            // Pro účely vývoje - vracíme vlastní kolekce s filtrováním
            var allCollections = await GetMyCollectionsAsync(token, cancellationToken);

            // Aplikace filtrů
            var filteredCollections = allCollections.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredCollections = filteredCollections.Where(c => 
                    c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Stránkování
            var pagedCollections = filteredCollections
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Načteno {Count} kolekcí ze {Total}", pagedCollections.Count, filteredCollections.Count());
            return pagedCollections;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání kolekcí");
            throw new CookidooException("Nepodařilo se načíst kolekce", ex);
        }
    }

    public async Task<List<string>> GetCollectionRecipesAsync(string token, string collectionId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání receptů kolekce {CollectionId} - stránka: {Page}, velikost: {PageSize}", collectionId, page, pageSize);

            var collection = await GetCollectionAsync(token, collectionId, true, cancellationToken);
            if (collection == null)
            {
                throw new CookidooNotFoundException($"Kolekce s ID {collectionId} nebyla nalezena");
            }

            // Stránkování receptů
            var pagedRecipeIds = collection.RecipeIds
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Načteno {Count} receptů ze {Total} v kolekci", pagedRecipeIds.Count, collection.RecipeIds.Count);
            return pagedRecipeIds;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání receptů kolekce");
            throw new CookidooException("Nepodařilo se načíst recepty kolekce", ex);
        }
    }
} 