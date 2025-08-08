using Microsoft.AspNetCore.Mvc;
using Cookidoo.MCP.Core.Interfaces;
using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Exceptions;
using Cookidoo.MCP.Api.Models.Collections;
using Cookidoo.MCP.Api.Mappers;
using Cookidoo.MCP.Api.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Controllers;

/// <summary>
/// Controller pro správu kolekcí receptů
/// Vyžaduje platný Cookidoo JWT token v Authorization headeru nebo query parametru
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    private readonly ICookidooApiService _cookidooApiService;
    private readonly ILogger<CollectionsController> _logger;

    public CollectionsController(
        ICollectionService collectionService, 
        ICookidooApiService cookidooApiService,
        ILogger<CollectionsController> logger)
    {
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
        _cookidooApiService = cookidooApiService ?? throw new ArgumentNullException(nameof(cookidooApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ověří platnost Cookidoo JWT tokenu
    /// </summary>
    /// <returns>JWT token nebo BadRequest pokud není platný</returns>
    private async Task<ActionResult<string>> ValidateTokenAsync()
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován. Poskytněte token v Authorization headeru (Bearer {token}) nebo query parametru 'jwt_token'.");
        }

        try
        {
            var isValid = await _cookidooApiService.ValidateTokenAsync(token);
            if (!isValid)
            {
                return Unauthorized("Cookidoo JWT token není platný nebo vypršel.");
            }

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při ověřování Cookidoo JWT tokenu");
            return StatusCode(500, "Chyba při ověřování autentizace s Cookidoo.");
        }
    }

    /// <summary>
    /// Získá seznam všech kolekcí
    /// </summary>
    /// <param name="page">Číslo stránky (výchozí: 1)</param>
    /// <param name="pageSize">Velikost stránky (výchozí: 20, max: 100)</param>
    /// <param name="search">Vyhledávací řetězec</param>
    /// <returns>Seznam kolekcí</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetCollections(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        // Ověření JWT tokenu
        var tokenResult = await ValidateTokenAsync();
        if (tokenResult.Result != null) return tokenResult.Result;
        var token = tokenResult.Value!;

        try
        {
            _logger.LogInformation("Získávám seznam kolekcí - stránka: {Page}, velikost: {PageSize}, hledání: {Search}", 
                page, pageSize, search);

            if (page < 1)
            {
                return BadRequest("Číslo stránky musí být větší než 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Velikost stránky musí být mezi 1 a 100");
            }

            var collections = await _collectionService.GetCollectionsAsync(token, page, pageSize, search);
            var collectionDtos = collections.Select(ApiMapper.ToDto);

            return Ok(collectionDtos);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při získávání kolekcí");
            return StatusCode(500, $"Chyba při získávání kolekcí: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při získávání kolekcí");
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Získá detail konkrétní kolekce
    /// </summary>
    /// <param name="id">ID kolekce</param>
    /// <returns>Detail kolekce</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> GetCollection([Required] string id)
    {
        try
        {
            _logger.LogInformation("Získávám detail kolekce s ID: {CollectionId}", id);

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var collection = await _collectionService.GetCollectionAsync(token, id);
            if (collection == null)
            {
                return NotFound($"Kolekce s ID '{id}' nebyla nalezena");
            }

            var collectionDto = ApiMapper.ToDto(collection);
            return Ok(collectionDto);
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Kolekce s ID {CollectionId} nebyla nalezena", id);
            return NotFound(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při získávání kolekce s ID {CollectionId}", id);
            return StatusCode(500, $"Chyba při získávání kolekce: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při získávání kolekce s ID {CollectionId}", id);
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Vytvoří novou kolekci
    /// </summary>
    /// <param name="createCollectionDto">Data pro vytvoření kolekce</param>
    /// <returns>Vytvořená kolekce</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> CreateCollection([FromBody] CreateCollectionDto createCollectionDto)
    {
        try
        {
            _logger.LogInformation("Vytvářím novou kolekci: {CollectionName}", createCollectionDto.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var collection = ApiMapper.ToEntity(createCollectionDto);
            var createdCollection = await _collectionService.CreateCollectionAsync(token, collection);
            var collectionDto = ApiMapper.ToDto(createdCollection);

            return CreatedAtAction(nameof(GetCollection), new { id = createdCollection.Id }, collectionDto);
        }
        catch (CookidooValidationException ex)
        {
            _logger.LogWarning(ex, "Validační chyba při vytváření kolekce");
            return BadRequest(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při vytváření kolekce");
            return StatusCode(500, $"Chyba při vytváření kolekce: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při vytváření kolekce");
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Aktualizuje existující kolekci
    /// </summary>
    /// <param name="id">ID kolekce</param>
    /// <param name="updateCollectionDto">Aktualizovaná data kolekce</param>
    /// <returns>Aktualizovaná kolekce</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> UpdateCollection([Required] string id, [FromBody] UpdateCollectionDto updateCollectionDto)
    {
        try
        {
            _logger.LogInformation("Aktualizuji kolekci s ID: {CollectionId}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var existingCollection = await _collectionService.GetCollectionAsync(token, id);
        if (existingCollection == null)
        {
            return NotFound(new { error = "Kolekce nebyla nalezena" });
        }
        
        ApiMapper.UpdateEntity(existingCollection, updateCollectionDto);
            var updatedCollection = await _collectionService.UpdateCollectionAsync(token, id, existingCollection);
            var collectionDto = ApiMapper.ToDto(updatedCollection);

            return Ok(collectionDto);
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Kolekce s ID {CollectionId} nebyla nalezena pro aktualizaci", id);
            return NotFound(ex.Message);
        }
        catch (CookidooValidationException ex)
        {
            _logger.LogWarning(ex, "Validační chyba při aktualizaci kolekce s ID {CollectionId}", id);
            return BadRequest(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při aktualizaci kolekce s ID {CollectionId}", id);
            return StatusCode(500, $"Chyba při aktualizaci kolekce: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při aktualizaci kolekce s ID {CollectionId}", id);
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Smaže kolekci
    /// </summary>
    /// <param name="id">ID kolekce</param>
    /// <returns>Potvrzení smazání</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCollection([Required] string id)
    {
        try
        {
            _logger.LogInformation("Mažu kolekci s ID: {CollectionId}", id);

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            await _collectionService.DeleteCollectionAsync(token, id);
            return NoContent();
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Kolekce s ID {CollectionId} nebyla nalezena pro smazání", id);
            return NotFound(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při mazání kolekce s ID {CollectionId}", id);
            return StatusCode(500, $"Chyba při mazání kolekce: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při mazání kolekce s ID {CollectionId}", id);
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Přidá recept do kolekce
    /// </summary>
    /// <param name="id">ID kolekce</param>
    /// <param name="addRecipeDto">Data pro přidání receptu</param>
    /// <returns>Aktualizovaná kolekce</returns>
    [HttpPost("{id}/recipes")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> AddRecipeToCollection([Required] string id, [FromBody] AddRecipeToCollectionDto addRecipeDto)
    {
        try
        {
            _logger.LogInformation("Přidávám recept {RecipeId} do kolekce {CollectionId}", addRecipeDto.RecipeId, id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var success = await _collectionService.AddRecipeToCollectionAsync(token, id, addRecipeDto.RecipeId);
            if (!success)
            {
                return StatusCode(500, "Nepodařilo se přidat recept do kolekce");
            }

            // Načteme aktualizovanou kolekci
            var updatedCollection = await _collectionService.GetCollectionAsync(token, id);
            var collectionDto = ApiMapper.ToDto(updatedCollection!);

            return Ok(collectionDto);
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Kolekce nebo recept nebyl nalezen");
            return NotFound(ex.Message);
        }
        catch (CookidooValidationException ex)
        {
            _logger.LogWarning(ex, "Validační chyba při přidávání receptu do kolekce");
            return BadRequest(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při přidávání receptu do kolekce");
            return StatusCode(500, $"Chyba při přidávání receptu do kolekce: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při přidávání receptu do kolekce");
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Odebere recept z kolekce
    /// </summary>
    /// <param name="id">ID kolekce</param>
    /// <param name="recipeId">ID receptu</param>
    /// <returns>Aktualizovaná kolekce</returns>
    [HttpDelete("{id}/recipes/{recipeId}")]
    [ProducesResponseType(typeof(CollectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> RemoveRecipeFromCollection([Required] string id, [Required] string recipeId)
    {
        try
        {
            _logger.LogInformation("Odebírám recept {RecipeId} z kolekce {CollectionId}", recipeId, id);

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var success = await _collectionService.RemoveRecipeFromCollectionAsync(token, id, recipeId);
            if (!success)
            {
                return StatusCode(500, "Nepodařilo se odebrat recept z kolekce");
            }

            // Načteme aktualizovanou kolekci
            var updatedCollection = await _collectionService.GetCollectionAsync(token, id);
            var collectionDto = ApiMapper.ToDto(updatedCollection!);

            return Ok(collectionDto);
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Kolekce nebo recept nebyl nalezen");
            return NotFound(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při odebírání receptu z kolekce");
            return StatusCode(500, $"Chyba při odebírání receptu z kolekce: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při odebírání receptu z kolekce");
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Získá recepty v kolekci
    /// </summary>
    /// <param name="id">ID kolekce</param>
    /// <param name="page">Číslo stránky (výchozí: 1)</param>
    /// <param name="pageSize">Velikost stránky (výchozí: 20, max: 100)</param>
    /// <returns>Seznam receptů v kolekci</returns>
    [HttpGet("{id}/recipes")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetCollectionRecipes(
        [Required] string id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Získávám recepty kolekce {CollectionId} - stránka: {Page}, velikost: {PageSize}", 
                id, page, pageSize);

            if (page < 1)
            {
                return BadRequest("Číslo stránky musí být větší než 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Velikost stránky musí být mezi 1 a 100");
            }

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var recipeIds = await _collectionService.GetCollectionRecipesAsync(token, id, page, pageSize);
            return Ok(recipeIds);
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Kolekce s ID {CollectionId} nebyla nalezena", id);
            return NotFound(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při získávání receptů kolekce s ID {CollectionId}", id);
            return StatusCode(500, $"Chyba při získávání receptů kolekce: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při získávání receptů kolekce s ID {CollectionId}", id);
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }
} 