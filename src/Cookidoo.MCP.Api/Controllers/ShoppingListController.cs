using Microsoft.AspNetCore.Mvc;
using Cookidoo.MCP.Api.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Controllers;

/// <summary>
/// Controller pro správu nákupního seznamu
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ShoppingListController : ControllerBase
{
    private readonly ILogger<ShoppingListController> _logger;

    public ShoppingListController(ILogger<ShoppingListController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Získá kompletní nákupní seznam
    /// </summary>
    /// <returns>Nákupní seznam s ingrediencemi z receptů a vlastními položkami</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingListResponse>> GetShoppingList()
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Načítání nákupního seznamu");

        // Mock data pro demonstraci
        var response = new ShoppingListResponse
        {
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    Id = "ing-1",
                    Text = "200g mouky",
                    RecipeName = "Čokoládový dort",
                    IsOwned = false
                },
                new RecipeIngredient
                {
                    Id = "ing-2",
                    Text = "3 vejce",
                    RecipeName = "Čokoládový dort",
                    IsOwned = true
                }
            },
            AdditionalItems = new List<ShoppingItem>
            {
                new ShoppingItem
                {
                    Id = "item-1",
                    Name = "Toaletní papír",
                    IsOwned = false
                }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Přidá ingredience z receptů do nákupního seznamu
    /// </summary>
    [HttpPost("recipes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddRecipesToShoppingList([FromBody] AddRecipesToShoppingListRequest request)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Přidávání receptů do nákupního seznamu: {RecipeIds}",
            string.Join(", ", request.RecipeIds));

        return Ok(new { message = $"Přidáno {request.RecipeIds.Count} receptů do nákupního seznamu" });
    }

    /// <summary>
    /// Odebere ingredience receptů z nákupního seznamu
    /// </summary>
    [HttpDelete("recipes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveRecipesFromShoppingList([FromBody] RemoveRecipesFromShoppingListRequest request)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Odebírání receptů z nákupního seznamu: {RecipeIds}",
            string.Join(", ", request.RecipeIds));

        return Ok(new { message = $"Odebráno {request.RecipeIds.Count} receptů z nákupního seznamu" });
    }

    /// <summary>
    /// Označí ingredience jako zakoupené
    /// </summary>
    [HttpPatch("ingredients/ownership")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkIngredientsAsOwned([FromBody] MarkIngredientsRequest request)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Označování ingrediencí jako zakoupených: {IngredientIds}",
            string.Join(", ", request.IngredientIds));

        return Ok(new { message = $"Označeno {request.IngredientIds.Count} ingrediencí jako zakoupených" });
    }

    /// <summary>
    /// Přidá vlastní položky do nákupního seznamu
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddShoppingItems([FromBody] AddShoppingItemsRequest request)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Přidávání vlastních položek: {Items}",
            string.Join(", ", request.Items));

        return Ok(new { message = $"Přidáno {request.Items.Count} položek" });
    }

    /// <summary>
    /// Odebere vlastní položky z nákupního seznamu
    /// </summary>
    [HttpDelete("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveShoppingItems([FromBody] RemoveShoppingItemsRequest request)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Odebírání položek: {ItemIds}",
            string.Join(", ", request.ItemIds));

        return Ok(new { message = $"Odebráno {request.ItemIds.Count} položek" });
    }

    /// <summary>
    /// Označí vlastní položky jako zakoupené
    /// </summary>
    [HttpPatch("items/ownership")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkItemsAsOwned([FromBody] MarkItemsRequest request)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Označování položek jako zakoupených: {ItemIds}",
            string.Join(", ", request.ItemIds));

        return Ok(new { message = $"Označeno {request.ItemIds.Count} položek jako zakoupených" });
    }

    /// <summary>
    /// Vymaže celý nákupní seznam
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearShoppingList()
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Mazání celého nákupního seznamu");

        return Ok(new { message = "Nákupní seznam byl vymazán" });
    }
}

// Request/Response models

public class ShoppingListResponse
{
    public List<RecipeIngredient> RecipeIngredients { get; set; } = new();
    public List<ShoppingItem> AdditionalItems { get; set; } = new();
}

public class RecipeIngredient
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public bool IsOwned { get; set; }
}

public class ShoppingItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsOwned { get; set; }
}

public class AddRecipesToShoppingListRequest
{
    [Required]
    public List<string> RecipeIds { get; set; } = new();
}

public class RemoveRecipesFromShoppingListRequest
{
    [Required]
    public List<string> RecipeIds { get; set; } = new();
}

public class MarkIngredientsRequest
{
    [Required]
    public List<string> IngredientIds { get; set; } = new();
}

public class AddShoppingItemsRequest
{
    [Required]
    public List<string> Items { get; set; } = new();
}

public class RemoveShoppingItemsRequest
{
    [Required]
    public List<string> ItemIds { get; set; } = new();
}

public class MarkItemsRequest
{
    [Required]
    public List<string> ItemIds { get; set; } = new();
}
