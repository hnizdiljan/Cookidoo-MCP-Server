using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Cookidoo.MCP.Infrastructure.Services;
using Cookidoo.MCP.Core.Interfaces;
using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Exceptions;

namespace Cookidoo.MCP.Tests.Services;

public class RecipeServiceTests
{
    private readonly Mock<ICookidooApiService> _cookidooApiServiceMock;
    private readonly Mock<ILogger<RecipeService>> _loggerMock;
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _cookidooApiServiceMock = new Mock<ICookidooApiService>();
        _loggerMock = new Mock<ILogger<RecipeService>>();
        _service = new RecipeService(_cookidooApiServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateRecipeAsync_WithValidRecipe_ReturnsCreatedRecipe()
    {
        // Arrange
        var token = "test-token";
        var recipe = CreateTestRecipe();
        var createdRecipe = CreateTestRecipe();
        createdRecipe.Id = "created-recipe-id";

        _cookidooApiServiceMock
            .Setup(x => x.CreateRecipeAsync(token, recipe, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRecipe);

        // Act
        var result = await _service.CreateRecipeAsync(token, recipe);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("created-recipe-id");
        result.Name.Should().Be(recipe.Name);
    }

    [Fact]
    public async Task CreateRecipeAsync_WithInvalidRecipe_ThrowsValidationException()
    {
        // Arrange
        var token = "test-token";
        var recipe = new Recipe
        {
            Name = "", // Invalid - empty name
            Description = "Test description",
            Ingredients = new List<Ingredient>(),
            Steps = new List<CookingStep>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<CookidooValidationException>(
            () => _service.CreateRecipeAsync(token, recipe));
    }

    [Fact]
    public async Task GetRecipeAsync_WithExistingId_ReturnsRecipe()
    {
        // Arrange
        var token = "test-token";
        var recipeId = "test-recipe-id";
        var recipe = CreateTestRecipe();
        recipe.Id = recipeId;

        _cookidooApiServiceMock
            .Setup(x => x.GetRecipeAsync(token, recipeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe);

        // Act
        var result = await _service.GetRecipeAsync(token, recipeId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(recipeId);
    }

    [Fact]
    public async Task GetRecipeAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var token = "test-token";
        var recipeId = "non-existing-id";

        _cookidooApiServiceMock
            .Setup(x => x.GetRecipeAsync(token, recipeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe?)null);

        // Act
        var result = await _service.GetRecipeAsync(token, recipeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRecipeAsync_WithValidRecipe_ReturnsUpdatedRecipe()
    {
        // Arrange
        var token = "test-token";
        var recipeId = "test-recipe-id";
        var existingRecipe = CreateTestRecipe();
        existingRecipe.Id = recipeId;
        
        var updatedRecipe = CreateTestRecipe();
        updatedRecipe.Id = recipeId;
        updatedRecipe.Name = "Updated Recipe Name";

        _cookidooApiServiceMock
            .Setup(x => x.GetRecipeAsync(token, recipeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecipe);

        _cookidooApiServiceMock
            .Setup(x => x.UpdateRecipeAsync(token, recipeId, It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedRecipe);

        // Act
        var result = await _service.UpdateRecipeAsync(token, recipeId, updatedRecipe);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Recipe Name");
    }

    [Fact]
    public async Task UpdateRecipeAsync_WithNonExistingRecipe_ThrowsNotFoundException()
    {
        // Arrange
        var token = "test-token";
        var recipeId = "non-existing-id";
        var recipe = CreateTestRecipe();

        _cookidooApiServiceMock
            .Setup(x => x.GetRecipeAsync(token, recipeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<CookidooNotFoundException>(
            () => _service.UpdateRecipeAsync(token, recipeId, recipe));
    }

    [Fact]
    public async Task ValidateRecipeAsync_WithValidRecipe_ReturnsNoErrors()
    {
        // Arrange
        var recipe = CreateTestRecipe();

        // Act
        var errors = await _service.ValidateRecipeAsync(recipe);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateRecipeAsync_WithInvalidRecipe_ReturnsErrors()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "", // Invalid
            Description = "Test",
            Ingredients = new List<Ingredient>(), // Invalid - no ingredients
            Steps = new List<CookingStep>(), // Invalid - no steps
            Portions = 0 // Invalid
        };

        // Act
        var errors = await _service.ValidateRecipeAsync(recipe);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("Název receptu je povinný"));
        errors.Should().Contain(e => e.Contains("alespoň jednu ingredienci"));
        errors.Should().Contain(e => e.Contains("alespoň jeden krok"));
        errors.Should().Contain(e => e.Contains("Počet porcí musí být mezi 1 a 50"));
    }

    private static Recipe CreateTestRecipe()
    {
        return new Recipe
        {
            Name = "Test Recipe",
            Description = "Test description",
            Ingredients = new List<Ingredient>
            {
                new() { Text = "Test ingredient 1" },
                new() { Text = "Test ingredient 2" }
            },
            Steps = new List<CookingStep>
            {
                new() { Text = "Test step 1" },
                new() { Text = "Test step 2" }
            },
            PreparationTimeMinutes = 30,
            CookingTimeMinutes = 45,
            Portions = 4,
            Difficulty = RecipeDifficulty.Medium,
            Tags = new List<string> { "test", "recipe" }
        };
    }
} 