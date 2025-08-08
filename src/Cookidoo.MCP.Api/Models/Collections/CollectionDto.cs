using System.ComponentModel.DataAnnotations;
using Cookidoo.MCP.Api.Models.Recipes;

namespace Cookidoo.MCP.Api.Models.Collections;

/// <summary>
/// DTO pro vytvoření nové kolekce
/// </summary>
public class CreateCollectionDto
{
    /// <summary>
    /// Název kolekce
    /// </summary>
    [Required(ErrorMessage = "Název kolekce je povinný")]
    [StringLength(200, ErrorMessage = "Název kolekce může mít maximálně 200 znaků")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Popis kolekce
    /// </summary>
    [StringLength(1000, ErrorMessage = "Popis kolekce může mít maximálně 1000 znaků")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Tagy kolekce
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// URL obrázku kolekce
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Zda je kolekce veřejná
    /// </summary>
    public bool IsPublic { get; set; } = false;
}

/// <summary>
/// DTO pro aktualizaci kolekce
/// </summary>
public class UpdateCollectionDto
{
    /// <summary>
    /// Název kolekce
    /// </summary>
    [StringLength(200, ErrorMessage = "Název kolekce může mít maximálně 200 znaků")]
    public string? Name { get; set; }

    /// <summary>
    /// Popis kolekce
    /// </summary>
    [StringLength(1000, ErrorMessage = "Popis kolekce může mít maximálně 1000 znaků")]
    public string? Description { get; set; }

    /// <summary>
    /// Tagy kolekce
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// URL obrázku kolekce
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Zda je kolekce veřejná
    /// </summary>
    public bool? IsPublic { get; set; }
}

/// <summary>
/// DTO pro zobrazení kolekce
/// </summary>
public class CollectionDto
{
    /// <summary>
    /// ID kolekce
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Název kolekce
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Popis kolekce
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Seznam ID receptů v kolekci
    /// </summary>
    public List<string> RecipeIds { get; set; } = new();

    /// <summary>
    /// Seznam receptů (pokud jsou načtené)
    /// </summary>
    public List<RecipeDto>? Recipes { get; set; }

    /// <summary>
    /// ID vlastníka kolekce
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Zda je kolekce veřejná
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Tagy kolekce
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// URL obrázku kolekce
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Datum vytvoření
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Datum poslední aktualizace
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Počet receptů v kolekci
    /// </summary>
    public int RecipeCount => RecipeIds.Count;
}

/// <summary>
/// DTO pro přidání receptu do kolekce
/// </summary>
public class AddRecipeToCollectionDto
{
    /// <summary>
    /// ID receptu k přidání
    /// </summary>
    [Required(ErrorMessage = "ID receptu je povinné")]
    public string RecipeId { get; set; } = string.Empty;
} 