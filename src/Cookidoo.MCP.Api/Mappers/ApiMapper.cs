using Cookidoo.MCP.Api.Models.Recipes;
using Cookidoo.MCP.Api.Models.Collections;
using Cookidoo.MCP.Core.Entities;

namespace Cookidoo.MCP.Api.Mappers;

/// <summary>
/// Mapper pro převod mezi API DTO a doménovými modely
/// </summary>
public static class ApiMapper
{
    #region Recipe Mapping

    /// <summary>
    /// Převede CreateRecipeDto na doménový model Recipe
    /// </summary>
    public static Recipe ToEntity(CreateRecipeDto dto)
    {
        return new Recipe
        {
            Name = dto.Name,
            Description = dto.Description,
            Ingredients = dto.Ingredients.Select(ToEntity).ToList(),
            Steps = dto.Steps.Select((step, index) => ToEntity(step, index + 1)).ToList(),
            PreparationTimeMinutes = dto.PreparationTimeMinutes,
            CookingTimeMinutes = dto.CookingTimeMinutes,
            Portions = dto.Portions,
            Difficulty = (RecipeDifficulty)dto.Difficulty,
            Tags = dto.Tags,
            ImageUrl = dto.ImageUrl,
            Notes = dto.Notes,
            IsPublic = dto.IsPublic,
            Tools = dto.Tools
        };
    }

    /// <summary>
    /// Aktualizuje doménový model Recipe podle UpdateRecipeDto
    /// </summary>
    public static void UpdateEntity(Recipe entity, UpdateRecipeDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Name))
            entity.Name = dto.Name;

        if (dto.Description != null)
            entity.Description = dto.Description;

        if (dto.Ingredients != null)
            entity.Ingredients = dto.Ingredients.Select(ToEntity).ToList();

        if (dto.Steps != null)
            entity.Steps = dto.Steps.Select((step, index) => ToEntity(step, index + 1)).ToList();

        if (dto.PreparationTimeMinutes.HasValue)
            entity.PreparationTimeMinutes = dto.PreparationTimeMinutes.Value;

        if (dto.CookingTimeMinutes.HasValue)
            entity.CookingTimeMinutes = dto.CookingTimeMinutes.Value;

        if (dto.Portions.HasValue)
            entity.Portions = dto.Portions.Value;

        if (dto.Difficulty.HasValue)
            entity.Difficulty = (RecipeDifficulty)dto.Difficulty.Value;

        if (dto.Tags != null)
            entity.Tags = dto.Tags;

        if (dto.ImageUrl != null)
            entity.ImageUrl = dto.ImageUrl;

        if (dto.Notes != null)
            entity.Notes = dto.Notes;

        if (dto.IsPublic.HasValue)
            entity.IsPublic = dto.IsPublic.Value;

        if (dto.Tools != null)
            entity.Tools = dto.Tools;

        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Převede doménový model Recipe na RecipeDto
    /// </summary>
    public static RecipeDto ToDto(Recipe entity)
    {
        return new RecipeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Ingredients = entity.Ingredients.Select(ToDto).ToList(),
            Steps = entity.Steps.OrderBy(s => s.Order).Select(ToDto).ToList(),
            PreparationTimeMinutes = entity.PreparationTimeMinutes,
            CookingTimeMinutes = entity.CookingTimeMinutes,
            Portions = entity.Portions,
            Difficulty = (RecipeDifficultyDto)entity.Difficulty,
            Tags = entity.Tags,
            ImageUrl = entity.ImageUrl,
            Notes = entity.Notes,
            CreatedBy = entity.CreatedBy,
            IsPublic = entity.IsPublic,
            Tools = entity.Tools,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// Převede IngredientDto na doménový model Ingredient
    /// </summary>
    public static Ingredient ToEntity(IngredientDto dto)
    {
        return new Ingredient
        {
            Text = dto.Text,
            Name = dto.Name,
            Quantity = dto.Quantity,
            Unit = dto.Unit
        };
    }

    /// <summary>
    /// Převede doménový model Ingredient na IngredientDto
    /// </summary>
    public static IngredientDto ToDto(Ingredient entity)
    {
        return new IngredientDto
        {
            Text = entity.Text,
            Name = entity.Name,
            Quantity = entity.Quantity,
            Unit = entity.Unit
        };
    }

    /// <summary>
    /// Převede CookingStepDto na doménový model CookingStep
    /// </summary>
    public static CookingStep ToEntity(CookingStepDto dto, int order)
    {
        return new CookingStep
        {
            Text = dto.Text,
            ImageUrl = dto.ImageUrl,
            Order = order
        };
    }

    /// <summary>
    /// Převede doménový model CookingStep na CookingStepDto
    /// </summary>
    public static CookingStepDto ToDto(CookingStep entity)
    {
        return new CookingStepDto
        {
            Text = entity.Text,
            ImageUrl = entity.ImageUrl,
            Order = entity.Order
        };
    }

    #endregion

    #region Collection Mapping

    /// <summary>
    /// Převede CreateCollectionDto na doménový model RecipeCollection
    /// </summary>
    public static RecipeCollection ToEntity(CreateCollectionDto dto)
    {
        return new RecipeCollection
        {
            Name = dto.Name,
            Description = dto.Description,
            Tags = dto.Tags,
            ImageUrl = dto.ImageUrl,
            IsPublic = dto.IsPublic
        };
    }

    /// <summary>
    /// Aktualizuje doménový model RecipeCollection podle UpdateCollectionDto
    /// </summary>
    public static void UpdateEntity(RecipeCollection entity, UpdateCollectionDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Name))
            entity.Name = dto.Name;

        if (dto.Description != null)
            entity.Description = dto.Description;

        if (dto.Tags != null)
            entity.Tags = dto.Tags;

        if (dto.ImageUrl != null)
            entity.ImageUrl = dto.ImageUrl;

        if (dto.IsPublic.HasValue)
            entity.IsPublic = dto.IsPublic.Value;

        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Převede doménový model RecipeCollection na CollectionDto
    /// </summary>
    public static CollectionDto ToDto(RecipeCollection entity)
    {
        return new CollectionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            RecipeIds = entity.RecipeIds,
            Recipes = entity.Recipes?.Select(ToDto).ToList(),
            OwnerId = entity.OwnerId,
            IsPublic = entity.IsPublic,
            Tags = entity.Tags,
            ImageUrl = entity.ImageUrl,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    #endregion

    #region List Mapping

    /// <summary>
    /// Převede seznam receptů na seznam DTO
    /// </summary>
    public static List<RecipeDto> ToDto(List<Recipe> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    /// <summary>
    /// Převede seznam kolekcí na seznam DTO
    /// </summary>
    public static List<CollectionDto> ToDto(List<RecipeCollection> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    #endregion
} 