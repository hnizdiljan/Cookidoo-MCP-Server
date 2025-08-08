using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Models.Recipes;

/// <summary>
/// DTO pro vytvoření nového receptu
/// </summary>
public class CreateRecipeDto
{
    /// <summary>
    /// Název receptu
    /// </summary>
    [Required(ErrorMessage = "Název receptu je povinný")]
    [StringLength(200, ErrorMessage = "Název receptu může mít maximálně 200 znaků")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Popis receptu
    /// </summary>
    [StringLength(2000, ErrorMessage = "Popis receptu může mít maximálně 2000 znaků")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Seznam ingrediencí
    /// </summary>
    [Required(ErrorMessage = "Recept musí obsahovat alespoň jednu ingredienci")]
    [MinLength(1, ErrorMessage = "Recept musí obsahovat alespoň jednu ingredienci")]
    public List<IngredientDto> Ingredients { get; set; } = new();

    /// <summary>
    /// Seznam kroků přípravy
    /// </summary>
    [Required(ErrorMessage = "Recept musí obsahovat alespoň jeden krok")]
    [MinLength(1, ErrorMessage = "Recept musí obsahovat alespoň jeden krok")]
    public List<CookingStepDto> Steps { get; set; } = new();

    /// <summary>
    /// Čas přípravy v minutách
    /// </summary>
    [Range(0, 1440, ErrorMessage = "Čas přípravy musí být mezi 0 a 1440 minutami")]
    public int PreparationTimeMinutes { get; set; }

    /// <summary>
    /// Čas vaření v minutách
    /// </summary>
    [Range(0, 1440, ErrorMessage = "Čas vaření musí být mezi 0 a 1440 minutami")]
    public int CookingTimeMinutes { get; set; }

    /// <summary>
    /// Počet porcí
    /// </summary>
    [Range(1, 50, ErrorMessage = "Počet porcí musí být mezi 1 a 50")]
    public int Portions { get; set; } = 4;

    /// <summary>
    /// Obtížnost receptu
    /// </summary>
    public RecipeDifficultyDto Difficulty { get; set; } = RecipeDifficultyDto.Medium;

    /// <summary>
    /// Tagy receptu
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// URL obrázku receptu
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Poznámky k receptu
    /// </summary>
    [StringLength(1000, ErrorMessage = "Poznámky mohou mít maximálně 1000 znaků")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Zda je recept veřejný
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Podporované nástroje
    /// </summary>
    public List<string> Tools { get; set; } = new();
}

/// <summary>
/// DTO pro aktualizaci receptu
/// </summary>
public class UpdateRecipeDto
{
    /// <summary>
    /// Název receptu
    /// </summary>
    [StringLength(200, ErrorMessage = "Název receptu může mít maximálně 200 znaků")]
    public string? Name { get; set; }

    /// <summary>
    /// Popis receptu
    /// </summary>
    [StringLength(2000, ErrorMessage = "Popis receptu může mít maximálně 2000 znaků")]
    public string? Description { get; set; }

    /// <summary>
    /// Seznam ingrediencí
    /// </summary>
    public List<IngredientDto>? Ingredients { get; set; }

    /// <summary>
    /// Seznam kroků přípravy
    /// </summary>
    public List<CookingStepDto>? Steps { get; set; }

    /// <summary>
    /// Čas přípravy v minutách
    /// </summary>
    [Range(0, 1440, ErrorMessage = "Čas přípravy musí být mezi 0 a 1440 minutami")]
    public int? PreparationTimeMinutes { get; set; }

    /// <summary>
    /// Čas vaření v minutách
    /// </summary>
    [Range(0, 1440, ErrorMessage = "Čas vaření musí být mezi 0 a 1440 minutami")]
    public int? CookingTimeMinutes { get; set; }

    /// <summary>
    /// Počet porcí
    /// </summary>
    [Range(1, 50, ErrorMessage = "Počet porcí musí být mezi 1 a 50")]
    public int? Portions { get; set; }

    /// <summary>
    /// Obtížnost receptu
    /// </summary>
    public RecipeDifficultyDto? Difficulty { get; set; }

    /// <summary>
    /// Tagy receptu
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// URL obrázku receptu
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Poznámky k receptu
    /// </summary>
    [StringLength(1000, ErrorMessage = "Poznámky mohou mít maximálně 1000 znaků")]
    public string? Notes { get; set; }

    /// <summary>
    /// Zda je recept veřejný
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// Podporované nástroje
    /// </summary>
    public List<string>? Tools { get; set; }
}

/// <summary>
/// DTO pro zobrazení receptu
/// </summary>
public class RecipeDto
{
    /// <summary>
    /// ID receptu
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Název receptu
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Popis receptu
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Seznam ingrediencí
    /// </summary>
    public List<IngredientDto> Ingredients { get; set; } = new();

    /// <summary>
    /// Seznam kroků přípravy
    /// </summary>
    public List<CookingStepDto> Steps { get; set; } = new();

    /// <summary>
    /// Čas přípravy v minutách
    /// </summary>
    public int PreparationTimeMinutes { get; set; }

    /// <summary>
    /// Čas vaření v minutách
    /// </summary>
    public int CookingTimeMinutes { get; set; }

    /// <summary>
    /// Celkový čas v minutách
    /// </summary>
    public int TotalTimeMinutes => PreparationTimeMinutes + CookingTimeMinutes;

    /// <summary>
    /// Počet porcí
    /// </summary>
    public int Portions { get; set; }

    /// <summary>
    /// Obtížnost receptu
    /// </summary>
    public RecipeDifficultyDto Difficulty { get; set; }

    /// <summary>
    /// Tagy receptu
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// URL obrázku receptu
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Poznámky k receptu
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// ID tvůrce receptu
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Zda je recept veřejný
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Podporované nástroje
    /// </summary>
    public List<string> Tools { get; set; } = new();

    /// <summary>
    /// Datum vytvoření
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Datum poslední aktualizace
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO pro ingredienci
/// </summary>
public class IngredientDto
{
    /// <summary>
    /// Textový popis ingredience včetně množství
    /// </summary>
    [Required(ErrorMessage = "Text ingredience je povinný")]
    [StringLength(200, ErrorMessage = "Text ingredience může mít maximálně 200 znaků")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Název ingredience
    /// </summary>
    [StringLength(100, ErrorMessage = "Název ingredience může mít maximálně 100 znaků")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Množství
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Jednotka měření
    /// </summary>
    [StringLength(50, ErrorMessage = "Jednotka může mít maximálně 50 znaků")]
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// DTO pro krok přípravy
/// </summary>
public class CookingStepDto
{
    /// <summary>
    /// Popis kroku
    /// </summary>
    [Required(ErrorMessage = "Popis kroku je povinný")]
    [StringLength(1000, ErrorMessage = "Popis kroku může mít maximálně 1000 znaků")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// URL obrázku pro krok
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Pořadí kroku
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// Obtížnost receptu
/// </summary>
public enum RecipeDifficultyDto
{
    /// <summary>
    /// Snadný
    /// </summary>
    Easy = 1,

    /// <summary>
    /// Střední
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Těžký
    /// </summary>
    Hard = 3
} 