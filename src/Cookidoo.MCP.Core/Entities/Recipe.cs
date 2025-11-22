using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Core.Entities;

/// <summary>
/// Doménový model pro recept v Cookidoo
/// </summary>
public class Recipe
{
    /// <summary>
    /// Unikátní identifikátor receptu v Cookidoo
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Název receptu
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Popis receptu
    /// </summary>
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Seznam ingrediencí
    /// </summary>
    public List<Ingredient> Ingredients { get; set; } = new();

    /// <summary>
    /// Seznam kroků přípravy
    /// </summary>
    public List<CookingStep> Steps { get; set; } = new();

    /// <summary>
    /// Čas přípravy v minutách
    /// </summary>
    [Range(0, 1440)] // Max 24 hodin
    public int PreparationTimeMinutes { get; set; }

    /// <summary>
    /// Čas vaření v minutách
    /// </summary>
    [Range(0, 1440)] // Max 24 hodin
    public int CookingTimeMinutes { get; set; }

    /// <summary>
    /// Celkový čas v sekundách (pro Cookidoo API)
    /// </summary>
    public int TotalTime => (PreparationTimeMinutes + CookingTimeMinutes) * 60;

    /// <summary>
    /// Čas přípravy v sekundách (pro Cookidoo API)
    /// </summary>
    public int PrepTime => PreparationTimeMinutes * 60;

    /// <summary>
    /// Počet porcí
    /// </summary>
    [Range(1, 50)]
    public int Portions { get; set; } = 4;

    /// <summary>
    /// Jednotka porcí (pro Cookidoo API)
    /// </summary>
    public RecipeYield Yield => new() { Value = Portions, UnitText = "portion" };

    /// <summary>
    /// Obtížnost receptu
    /// </summary>
    public RecipeDifficulty Difficulty { get; set; } = RecipeDifficulty.Medium;

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
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Nutriční informace
    /// </summary>
    public NutritionalInfo? NutritionalInfo { get; set; }

    /// <summary>
    /// ID tvůrce receptu
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Zda je recept veřejný na Cookidoo
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Podporované nástroje (např. TM6)
    /// </summary>
    public List<string> Tools { get; set; } = new();

    /// <summary>
    /// Datum vytvoření
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Datum poslední aktualizace
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Model ingredience
/// </summary>
public class Ingredient
{
    /// <summary>
    /// Typ položky (pro Cookidoo API vždy "INGREDIENT")
    /// </summary>
    public string Type { get; set; } = "INGREDIENT";

    /// <summary>
    /// Textový popis ingredience včetně množství
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Název ingredience
    /// </summary>
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Množství
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Jednotka měření
    /// </summary>
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// Model kroku přípravy
/// </summary>
public class CookingStep
{
    /// <summary>
    /// Typ položky (pro Cookidoo API vždy "STEP")
    /// </summary>
    public string Type { get; set; } = "STEP";

    /// <summary>
    /// Popis kroku
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// URL obrázku pro krok
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Pořadí kroku
    /// </summary>
    public int Order { get; set; }

    // === Thermomix specifické parametry ===

    /// <summary>
    /// Čas v sekundách (např. 90 pro 1,5 minuty)
    /// </summary>
    public int? TimeSeconds { get; set; }

    /// <summary>
    /// Teplota v °C (0-120, null = bez ohřevu)
    /// </summary>
    [Range(0, 120)]
    public int? Temperature { get; set; }

    /// <summary>
    /// Rychlost mixéru (1-10, nebo speciální hodnoty)
    /// Thermomix TM6: 1-10, plus speciální režimy
    /// </summary>
    [Range(0, 10)]
    public int? Speed { get; set; }

    /// <summary>
    /// Použití vážení ("Turbo" rychlost)
    /// </summary>
    public bool? UseTurbo { get; set; }

    /// <summary>
    /// Použití levo-otáček (šetrné míchání)
    /// </summary>
    public bool? UseReverseRotation { get; set; }

    /// <summary>
    /// Použití Varoma režimu (pro vaření v páře)
    /// </summary>
    public bool? UseVaroma { get; set; }

    /// <summary>
    /// Generuje formátovaný text pro Cookidoo API
    /// Formát: "{čas}/{teplota}/{rychlost} {text}"
    /// Příklad: "6 Min./100°C/Stufe 2 kochen"
    /// </summary>
    public string GetFormattedText()
    {
        if (!TimeSeconds.HasValue && !Temperature.HasValue && !Speed.HasValue)
        {
            // Žádné Thermomix parametry - vrátíme jen text
            return Text;
        }

        var parts = new List<string>();

        // Čas
        if (TimeSeconds.HasValue)
        {
            var seconds = TimeSeconds.Value;
            if (seconds < 60)
            {
                parts.Add($"{seconds} Sek.");
            }
            else
            {
                var minutes = seconds / 60;
                var remainingSeconds = seconds % 60;
                if (remainingSeconds > 0)
                {
                    parts.Add($"{minutes} Min. {remainingSeconds} Sek.");
                }
                else
                {
                    parts.Add($"{minutes} Min.");
                }
            }
        }

        // Teplota
        if (Temperature.HasValue)
        {
            if (UseVaroma == true)
            {
                parts.Add("Varoma");
            }
            else
            {
                parts.Add($"{Temperature}°C");
            }
        }

        // Rychlost
        if (Speed.HasValue)
        {
            if (UseTurbo == true)
            {
                parts.Add("Turbo");
            }
            else
            {
                var speedText = $"Stufe {Speed}";
                if (UseReverseRotation == true)
                {
                    speedText = $"{speedText} Linkslauf";
                }
                parts.Add(speedText);
            }
        }

        var thermomixParams = string.Join("/", parts);
        return $"<nobr>{thermomixParams}</nobr> {Text}";
    }
}

/// <summary>
/// Model výtěžnosti receptu
/// </summary>
public class RecipeYield
{
    /// <summary>
    /// Hodnota (počet porcí)
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Jednotka (např. "portion")
    /// </summary>
    public string UnitText { get; set; } = "portion";
}

/// <summary>
/// Nutriční informace
/// </summary>
public class NutritionalInfo
{
    /// <summary>
    /// Kalorie na porci
    /// </summary>
    public int? CaloriesPerServing { get; set; }

    /// <summary>
    /// Bílkoviny v gramech
    /// </summary>
    public decimal? Protein { get; set; }

    /// <summary>
    /// Sacharidy v gramech
    /// </summary>
    public decimal? Carbohydrates { get; set; }

    /// <summary>
    /// Tuky v gramech
    /// </summary>
    public decimal? Fat { get; set; }

    /// <summary>
    /// Vláknina v gramech
    /// </summary>
    public decimal? Fiber { get; set; }
}

/// <summary>
/// Obtížnost receptu
/// </summary>
public enum RecipeDifficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3
} 