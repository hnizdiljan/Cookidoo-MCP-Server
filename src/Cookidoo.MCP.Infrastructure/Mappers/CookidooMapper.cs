using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Interfaces;
using Cookidoo.MCP.Infrastructure.Models;

namespace Cookidoo.MCP.Infrastructure.Mappers;

/// <summary>
/// Mapper pro převod mezi doménovými modely a DTO pro Cookidoo API
/// </summary>
public static class CookidooMapper
{
    /// <summary>
    /// Převede doménový model receptu na DTO pro vytvoření
    /// </summary>
    public static CreateRecipeRequest ToCreateRequest(Recipe recipe)
    {
        return new CreateRecipeRequest
        {
            RecipeName = recipe.Name
        };
    }

    /// <summary>
    /// Převede doménový model receptu na DTO pro aktualizaci
    /// </summary>
    public static UpdateRecipeRequest ToUpdateRequest(Recipe recipe)
    {
        return new UpdateRecipeRequest
        {
            RecipeName = recipe.Name,
            Description = recipe.Description,
            Ingredients = recipe.Ingredients.Select(ToIngredientDto).ToList(),
            Instructions = recipe.Steps.OrderBy(s => s.Order).Select(ToInstructionDto).ToList(),
            Tools = recipe.Tools,
            TotalTime = recipe.TotalTime,
            PrepTime = recipe.PrepTime,
            Yield = new CookidooYieldDto { Value = recipe.Portions, UnitText = "portion" },
            Tags = recipe.Tags,
            ImageUrl = recipe.ImageUrl,
            Notes = recipe.Notes,
            IsPublic = recipe.IsPublic
        };
    }

    /// <summary>
    /// Převede ingredienci na DTO
    /// </summary>
    public static CookidooIngredientDto ToIngredientDto(Ingredient ingredient)
    {
        return new CookidooIngredientDto
        {
            Type = "INGREDIENT",
            Text = ingredient.Text
        };
    }

    /// <summary>
    /// Převede krok přípravy na DTO
    /// Automaticky formátuje Thermomix parametry do správného formátu
    /// </summary>
    public static CookidooInstructionDto ToInstructionDto(CookingStep step)
    {
        return new CookidooInstructionDto
        {
            Type = "STEP",
            Text = step.GetFormattedText() // Použije GetFormattedText() pro správné formátování Thermomix parametrů
        };
    }

    /// <summary>
    /// Převede DTO receptu na doménový model
    /// </summary>
    public static Recipe FromRecipeDto(CookidooRecipeDto dto)
    {
        return new Recipe
        {
            Id = dto.RecipeId,
            Name = dto.RecipeName,
            Description = dto.Description ?? string.Empty,
            Ingredients = dto.Ingredients.Select(FromIngredientDto).ToList(),
            Steps = dto.Instructions.Select((instruction, index) => FromInstructionDto(instruction, index)).ToList(),
            Tools = dto.Tools,
            PreparationTimeMinutes = dto.PrepTime.HasValue ? dto.PrepTime.Value / 60 : 0,
            CookingTimeMinutes = dto.TotalTime.HasValue && dto.PrepTime.HasValue 
                ? (dto.TotalTime.Value - dto.PrepTime.Value) / 60 
                : 0,
            Portions = dto.Yield?.Value ?? 4,
            Tags = dto.Tags,
            ImageUrl = dto.ImageUrl,
            Notes = dto.Notes ?? string.Empty,
            IsPublic = dto.IsPublic,
            CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = dto.UpdatedAt ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Převede DTO ingredience na doménový model
    /// </summary>
    public static Ingredient FromIngredientDto(CookidooIngredientDto dto)
    {
        return new Ingredient
        {
            Type = dto.Type,
            Text = dto.Text
        };
    }

    /// <summary>
    /// Převede DTO instrukce na doménový model
    /// </summary>
    public static CookingStep FromInstructionDto(CookidooInstructionDto dto, int order)
    {
        return new CookingStep
        {
            Type = dto.Type,
            Text = dto.Text,
            Order = order + 1
        };
    }

    /// <summary>
    /// Převede doménový model kolekce na DTO pro vytvoření
    /// </summary>
    public static CreateCollectionRequest ToCreateCollectionRequest(RecipeCollection collection)
    {
        return new CreateCollectionRequest
        {
            Name = collection.Name,
            Description = collection.Description
        };
    }

    /// <summary>
    /// Převede DTO kolekce na doménový model
    /// </summary>
    public static RecipeCollection FromCollectionDto(CookidooCollectionDto dto)
    {
        return new RecipeCollection
        {
            Id = dto.CollectionId,
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            RecipeIds = dto.RecipeIds,
            Recipes = dto.Recipes?.Select(FromRecipeDto).ToList(),
            Tags = dto.Tags,
            ImageUrl = dto.ImageUrl,
            IsPublic = dto.IsPublic,
            CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = dto.UpdatedAt ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Převede DTO uživatele na doménový model
    /// </summary>
    public static CookidooUser FromUserDto(CookidooUserDto dto)
    {
        return new CookidooUser
        {
            Id = dto.UserId,
            Email = dto.Email,
            Name = dto.Name,
            Language = dto.Language
        };
    }

    /// <summary>
    /// Převede request pro přidání receptu do kolekce
    /// </summary>
    public static AddRecipeToCollectionRequest ToAddRecipeRequest(string recipeId)
    {
        return new AddRecipeToCollectionRequest
        {
            RecipeId = recipeId
        };
    }
} 