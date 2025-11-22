using Microsoft.AspNetCore.Mvc;
using Cookidoo.MCP.Api.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Controllers;

/// <summary>
/// Controller pro plánování jídel
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class MealPlanController : ControllerBase
{
    private readonly ILogger<MealPlanController> _logger;

    public MealPlanController(ILogger<MealPlanController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Získá plán jídel pro daný týden
    /// </summary>
    /// <param name="date">Datum v týdnu (formát: YYYY-MM-DD)</param>
    /// <returns>Plán jídel pro celý týden</returns>
    [HttpGet("week")]
    [ProducesResponseType(typeof(WeeklyMealPlanResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WeeklyMealPlanResponse>> GetWeeklyMealPlan([FromQuery] string? date = null)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        var targetDate = string.IsNullOrEmpty(date) ? DateTime.Now : DateTime.Parse(date);
        _logger.LogInformation("Načítání plánu jídel pro týden obsahující: {Date}", targetDate);

        // Mock data pro demonstraci
        var response = new WeeklyMealPlanResponse
        {
            WeekStart = targetDate.AddDays(-(int)targetDate.DayOfWeek + 1),
            WeekEnd = targetDate.AddDays(7 - (int)targetDate.DayOfWeek),
            Days = new List<DayMealPlan>
            {
                new DayMealPlan
                {
                    Date = targetDate.AddDays(-(int)targetDate.DayOfWeek + 1),
                    DayName = "Pondělí",
                    Meals = new List<PlannedMeal>
                    {
                        new PlannedMeal
                        {
                            RecipeId = "recipe-1",
                            RecipeName = "Špagety Carbonara",
                            MealType = "Oběd",
                            TotalTime = 30
                        }
                    }
                }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Získá plán jídel pro konkrétní den
    /// </summary>
    [HttpGet("day")]
    [ProducesResponseType(typeof(DayMealPlan), StatusCodes.Status200OK)]
    public async Task<ActionResult<DayMealPlan>> GetDayMealPlan([FromQuery] string date)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        var targetDate = DateTime.Parse(date);
        _logger.LogInformation("Načítání plánu jídel pro den: {Date}", targetDate);

        var response = new DayMealPlan
        {
            Date = targetDate,
            DayName = targetDate.ToString("dddd"),
            Meals = new List<PlannedMeal>()
        };

        return Ok(response);
    }

    /// <summary>
    /// Přidá recepty do plánu jídel
    /// </summary>
    [HttpPost("recipes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddRecipesToMealPlan([FromBody] AddRecipesToMealPlanRequest request)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        _logger.LogInformation("Přidávání receptů do plánu: {Date}, Recepty: {RecipeIds}",
            request.Date, string.Join(", ", request.RecipeIds));

        return Ok(new {
            message = $"Přidáno {request.RecipeIds.Count} receptů do plánu na {request.Date:dd.MM.yyyy}"
        });
    }

    /// <summary>
    /// Odebere recept z plánu jídel
    /// </summary>
    [HttpDelete("recipes/{recipeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveRecipeFromMealPlan(string recipeId, [FromQuery] string date)
    {
        var token = this.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Cookidoo JWT token je vyžadován");
        }

        var targetDate = DateTime.Parse(date);
        _logger.LogInformation("Odebírání receptu {RecipeId} z plánu pro {Date}", recipeId, targetDate);

        return Ok(new { message = $"Recept odebrán z plánu pro {targetDate:dd.MM.yyyy}" });
    }
}

// Response models

public class WeeklyMealPlanResponse
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<DayMealPlan> Days { get; set; } = new();
}

public class DayMealPlan
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public List<PlannedMeal> Meals { get; set; } = new();
}

public class PlannedMeal
{
    public string RecipeId { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty; // Oběd, Večeře, Snídaně
    public int TotalTime { get; set; } // v minutách
}

public class AddRecipesToMealPlanRequest
{
    [Required]
    public DateTime Date { get; set; }

    [Required]
    public List<string> RecipeIds { get; set; } = new();

    public string? MealType { get; set; } // Volitelně: Oběd, Večeře, Snídaně
}
