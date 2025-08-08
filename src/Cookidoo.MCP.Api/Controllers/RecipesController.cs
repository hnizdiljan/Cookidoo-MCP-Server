using Microsoft.AspNetCore.Mvc;
using Cookidoo.MCP.Core.Interfaces;
using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Exceptions;
using Cookidoo.MCP.Api.Models.Recipes;
using Cookidoo.MCP.Api.Mappers;
using Cookidoo.MCP.Api.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Controllers;

/// <summary>
/// Controller pro správu receptů
/// Vyžaduje platný Cookidoo JWT token v Authorization headeru nebo query parametru
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    private readonly ICookidooApiService _cookidooApiService;
    private readonly ILogger<RecipesController> _logger;

    public RecipesController(
        IRecipeService recipeService, 
        ICookidooApiService cookidooApiService,
        ILogger<RecipesController> logger)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
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
    /// Získá seznam všech receptů
    /// </summary>
    /// <param name="page">Číslo stránky (výchozí: 1)</param>
    /// <param name="pageSize">Velikost stránky (výchozí: 20, max: 100)</param>
    /// <param name="search">Vyhledávací řetězec</param>
    /// <param name="difficulty">Filtr podle obtížnosti</param>
    /// <param name="tags">Filtr podle tagů (oddělené čárkou)</param>
    /// <returns>Seznam receptů</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RecipeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetRecipes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] RecipeDifficulty? difficulty = null,
        [FromQuery] string? tags = null)
    {
        // Ověření JWT tokenu
        var tokenResult = await ValidateTokenAsync();
        if (tokenResult.Result != null) return tokenResult.Result;
        var token = tokenResult.Value!;

        try
        {
            _logger.LogInformation("Získávám seznam receptů - stránka: {Page}, velikost: {PageSize}, hledání: {Search}", 
                page, pageSize, search);

            if (page < 1)
            {
                return BadRequest("Číslo stránky musí být větší než 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Velikost stránky musí být mezi 1 a 100");
            }

            var tagList = string.IsNullOrWhiteSpace(tags) 
                ? new List<string>() 
                : tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

            var recipes = await _recipeService.GetRecipesAsync(token, page, pageSize, search, difficulty, tagList);
            var recipeDtos = recipes.Select(ApiMapper.ToDto);

            return Ok(recipeDtos);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při získávání receptů");
            return StatusCode(500, $"Chyba při získávání receptů: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při získávání receptů");
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Získá detail konkrétního receptu
    /// </summary>
    /// <param name="id">ID receptu</param>
    /// <returns>Detail receptu</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RecipeDto>> GetRecipe([Required] string id)
    {
        // Ověření JWT tokenu
        var tokenResult = await ValidateTokenAsync();
        if (tokenResult.Result != null) return tokenResult.Result;
        var token = tokenResult.Value!;

        try
        {
            _logger.LogInformation("Získávám detail receptu s ID: {RecipeId}", id);

            var recipe = await _recipeService.GetRecipeAsync(token, id);
            if (recipe == null)
            {
                return NotFound($"Recept s ID '{id}' nebyl nalezen");
            }

            var recipeDto = ApiMapper.ToDto(recipe);
            return Ok(recipeDto);
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recept s ID {RecipeId} nebyl nalezen", id);
            return NotFound(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při získávání receptu s ID {RecipeId}", id);
            return StatusCode(500, $"Chyba při získávání receptu: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při získávání receptu s ID {RecipeId}", id);
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Vytvoří nový recept
    /// </summary>
    /// <param name="createRecipeDto">Data pro vytvoření receptu</param>
    /// <returns>Vytvořený recept</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RecipeDto>> CreateRecipe([FromBody] CreateRecipeDto createRecipeDto)
    {
        // Ověření JWT tokenu
        var tokenResult = await ValidateTokenAsync();
        if (tokenResult.Result != null) return tokenResult.Result;
        var token = tokenResult.Value!;

        try
        {
            _logger.LogInformation("Vytvářím nový recept: {RecipeName}", createRecipeDto.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var recipe = ApiMapper.ToEntity(createRecipeDto);
            var createdRecipe = await _recipeService.CreateRecipeAsync(token, recipe);
            var recipeDto = ApiMapper.ToDto(createdRecipe);

            return CreatedAtAction(nameof(GetRecipe), new { id = createdRecipe.Id }, recipeDto);
        }
        catch (CookidooValidationException ex)
        {
            _logger.LogWarning(ex, "Validační chyba při vytváření receptu");
            return BadRequest(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při vytváření receptu");
            return StatusCode(500, $"Chyba při vytváření receptu: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při vytváření receptu");
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Aktualizuje existující recept
    /// </summary>
    /// <param name="id">ID receptu</param>
    /// <param name="updateRecipeDto">Aktualizovaná data receptu</param>
    /// <returns>Aktualizovaný recept</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RecipeDto>> UpdateRecipe([Required] string id, [FromBody] UpdateRecipeDto updateRecipeDto)
    {
        try
        {
            _logger.LogInformation("Aktualizuji recept s ID: {RecipeId}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var existingRecipe = await _recipeService.GetRecipeAsync(token, id);
        if (existingRecipe == null)
        {
            return NotFound(new { error = "Recept nebyl nalezen" });
        }
        
        ApiMapper.UpdateEntity(existingRecipe, updateRecipeDto);
            var updatedRecipe = await _recipeService.UpdateRecipeAsync(token, id, existingRecipe);
            var recipeDto = ApiMapper.ToDto(updatedRecipe);

            return Ok(recipeDto);
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recept s ID {RecipeId} nebyl nalezen pro aktualizaci", id);
            return NotFound(ex.Message);
        }
        catch (CookidooValidationException ex)
        {
            _logger.LogWarning(ex, "Validační chyba při aktualizaci receptu s ID {RecipeId}", id);
            return BadRequest(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při aktualizaci receptu s ID {RecipeId}", id);
            return StatusCode(500, $"Chyba při aktualizaci receptu: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při aktualizaci receptu s ID {RecipeId}", id);
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Smaže recept
    /// </summary>
    /// <param name="id">ID receptu</param>
    /// <returns>Potvrzení smazání</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRecipe([Required] string id)
    {
        try
        {
            _logger.LogInformation("Mažu recept s ID: {RecipeId}", id);

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            await _recipeService.DeleteRecipeAsync(token, id);
            return NoContent();
        }
        catch (CookidooNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recept s ID {RecipeId} nebyl nalezen pro smazání", id);
            return NotFound(ex.Message);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při mazání receptu s ID {RecipeId}", id);
            return StatusCode(500, $"Chyba při mazání receptu: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při mazání receptu s ID {RecipeId}", id);
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Vyhledá recepty podle kritérií
    /// </summary>
    /// <param name="query">Vyhledávací dotaz</param>
    /// <param name="maxResults">Maximální počet výsledků (výchozí: 50)</param>
    /// <returns>Seznam nalezených receptů</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<RecipeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> SearchRecipes(
        [FromQuery, Required] string query,
        [FromQuery] int maxResults = 50)
    {
        try
        {
            _logger.LogInformation("Vyhledávám recepty podle dotazu: {Query}", query);

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Vyhledávací dotaz nesmí být prázdný");
            }

            if (maxResults < 1 || maxResults > 100)
            {
                return BadRequest("Maximální počet výsledků musí být mezi 1 a 100");
            }

            var token = this.GetCookidooToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Cookidoo token není k dispozici");
            }

            var recipes = await _recipeService.SearchRecipesAsync(token, query, maxResults);
            var recipeDtos = recipes.Select(ApiMapper.ToDto);

            return Ok(recipeDtos);
        }
        catch (CookidooException ex)
        {
            _logger.LogError(ex, "Chyba při vyhledávání receptů");
            return StatusCode(500, $"Chyba při vyhledávání receptů: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při vyhledávání receptů");
            return StatusCode(500, "Došlo k neočekávané chybě");
        }
    }
} 