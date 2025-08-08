using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Core.Entities;

/// <summary>
/// Doménový model pro kolekci receptů v Cookidoo
/// </summary>
public class RecipeCollection
{
    /// <summary>
    /// Unikátní identifikátor kolekce v Cookidoo
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Název kolekce
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Popis kolekce
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Seznam ID receptů v kolekci
    /// </summary>
    public List<string> RecipeIds { get; set; } = new();

    /// <summary>
    /// Seznam receptů (načtených z Cookidoo)
    /// </summary>
    public List<Recipe>? Recipes { get; set; }

    /// <summary>
    /// ID vlastníka kolekce
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Zda je kolekce veřejná na Cookidoo
    /// </summary>
    public bool IsPublic { get; set; } = false;

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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Datum poslední aktualizace
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Počet receptů v kolekci
    /// </summary>
    public int RecipeCount => RecipeIds.Count;
} 