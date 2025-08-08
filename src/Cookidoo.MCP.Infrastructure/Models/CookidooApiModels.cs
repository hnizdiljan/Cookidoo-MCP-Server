using System.Text.Json.Serialization;

namespace Cookidoo.MCP.Infrastructure.Models;

/// <summary>
/// DTO pro vytvoření nového receptu v Cookidoo API
/// </summary>
public class CreateRecipeRequest
{
    [JsonPropertyName("recipeName")]
    public string RecipeName { get; set; } = string.Empty;
}

/// <summary>
/// DTO pro odpověď při vytvoření receptu
/// </summary>
public class CreateRecipeResponse
{
    [JsonPropertyName("recipeId")]
    public string RecipeId { get; set; } = string.Empty;

    [JsonPropertyName("recipeName")]
    public string RecipeName { get; set; } = string.Empty;
}

/// <summary>
/// DTO pro aktualizaci receptu
/// </summary>
public class UpdateRecipeRequest
{
    [JsonPropertyName("ingredients")]
    public List<CookidooIngredientDto>? Ingredients { get; set; }

    [JsonPropertyName("instructions")]
    public List<CookidooInstructionDto>? Instructions { get; set; }

    [JsonPropertyName("tools")]
    public List<string>? Tools { get; set; }

    [JsonPropertyName("totalTime")]
    public int? TotalTime { get; set; }

    [JsonPropertyName("prepTime")]
    public int? PrepTime { get; set; }

    [JsonPropertyName("yield")]
    public CookidooYieldDto? Yield { get; set; }

    [JsonPropertyName("recipeName")]
    public string? RecipeName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("isPublic")]
    public bool? IsPublic { get; set; }
}

/// <summary>
/// DTO pro ingredienci v Cookidoo API
/// </summary>
public class CookidooIngredientDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "INGREDIENT";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// DTO pro instrukci/krok v Cookidoo API
/// </summary>
public class CookidooInstructionDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "STEP";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// DTO pro výtěžnost receptu v Cookidoo API
/// </summary>
public class CookidooYieldDto
{
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("unitText")]
    public string UnitText { get; set; } = "portion";
}

/// <summary>
/// DTO pro detail receptu z Cookidoo API
/// </summary>
public class CookidooRecipeDto
{
    [JsonPropertyName("recipeId")]
    public string RecipeId { get; set; } = string.Empty;

    [JsonPropertyName("recipeName")]
    public string RecipeName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("ingredients")]
    public List<CookidooIngredientDto> Ingredients { get; set; } = new();

    [JsonPropertyName("instructions")]
    public List<CookidooInstructionDto> Instructions { get; set; } = new();

    [JsonPropertyName("tools")]
    public List<string> Tools { get; set; } = new();

    [JsonPropertyName("totalTime")]
    public int? TotalTime { get; set; }

    [JsonPropertyName("prepTime")]
    public int? PrepTime { get; set; }

    [JsonPropertyName("yield")]
    public CookidooYieldDto? Yield { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO pro seznam receptů
/// </summary>
public class CookidooRecipeListResponse
{
    [JsonPropertyName("recipes")]
    public List<CookidooRecipeDto> Recipes { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

/// <summary>
/// DTO pro vytvoření kolekce
/// </summary>
public class CreateCollectionRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// DTO pro kolekci z Cookidoo API
/// </summary>
public class CookidooCollectionDto
{
    [JsonPropertyName("collectionId")]
    public string CollectionId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("recipeIds")]
    public List<string> RecipeIds { get; set; } = new();

    [JsonPropertyName("recipes")]
    public List<CookidooRecipeDto>? Recipes { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO pro seznam kolekcí
/// </summary>
public class CookidooCollectionListResponse
{
    [JsonPropertyName("collections")]
    public List<CookidooCollectionDto> Collections { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

/// <summary>
/// DTO pro přidání receptu do kolekce
/// </summary>
public class AddRecipeToCollectionRequest
{
    [JsonPropertyName("recipeId")]
    public string RecipeId { get; set; } = string.Empty;
}

/// <summary>
/// DTO pro informace o uživateli
/// </summary>
public class CookidooUserDto
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = "de-DE";
}

/// <summary>
/// DTO pro chybovou odpověď z Cookidoo API
/// </summary>
public class CookidooErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("details")]
    public object? Details { get; set; }
} 