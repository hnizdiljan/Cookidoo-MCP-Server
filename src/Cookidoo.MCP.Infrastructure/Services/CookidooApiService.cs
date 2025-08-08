using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Exceptions;
using Cookidoo.MCP.Core.Interfaces;
using Cookidoo.MCP.Infrastructure.Configuration;
using Cookidoo.MCP.Infrastructure.Mappers;
using Cookidoo.MCP.Infrastructure.Models;

namespace Cookidoo.MCP.Infrastructure.Services;

/// <summary>
/// Implementace komunikace s Cookidoo API inspirovaná projekty cookiput a cookidoo-api
/// </summary>
public class CookidooApiService : ICookidooApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CookidooApiService> _logger;
    private readonly CookidooOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public CookidooApiService(
        HttpClient httpClient,
        ILogger<CookidooApiService> logger,
        IOptions<CookidooOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        // Používáme stejný User-Agent jako v cookiput-main projektu
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("troet");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserInfoAsync(token, cancellationToken);
            return user != null;
        }
        catch (CookidooAuthenticationException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Chyba při validaci tokenu");
            return false;
        }
    }

    public async Task<CookidooUser?> GetUserInfoAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Nejprve zkusíme parsovat mock token
            if (TryParseMockToken(token, out var mockUserInfo))
            {
                _logger.LogDebug("Používám mock uživatelské informace z tokenu");
                return mockUserInfo;
            }

            // Pro validaci tokenu zkusíme jednoduché volání na Cookidoo API podobně jako cookiput
            // Použijeme endpoint pro vlastní recepty, protože je dostupný a rychlý
            var request = CreateAuthenticatedRequest(HttpMethod.Get, "/created-recipes", token);
            var response = await SendRequestAsync(request, cancellationToken);
            
            // Pokud dostaneme úspěšnou odpověď, token je platný
            if (response != null)
            {
                // Vrátime základní uživatelské informace
                return new CookidooUser
                {
                    Id = "cookidoo-user",
                    Email = "user@cookidoo.de",
                    Name = "Cookidoo User",
                    Language = _options.DefaultLanguage
                };
            }
            
            return null;
        }
        catch (CookidooAuthenticationException)
        {
            _logger.LogWarning("Autentizace selhala při získávání informací o uživateli");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při získávání informací o uživateli");
            throw new CookidooAuthenticationException("Nepodařilo se získat informace o uživateli", ex);
        }
    }

    /// <summary>
    /// Pokusí se parsovat mock OAuth2 proxy token a extrahovat uživatelské informace
    /// </summary>
    private bool TryParseMockToken(string token, out CookidooUser? userInfo)
    {
        userInfo = null;
        
        // Nejprve zkusíme jednoduché mock tokeny pro testování
        if (token.StartsWith("mock-") || token == "test-token" || token == "demo-token")
        {
            userInfo = new CookidooUser
            {
                Id = "mock-user-123",
                Email = "test@cookidoo.de",
                Name = "Mock Test User",
                Language = "cs-CZ"
            };
            return true;
        }
        
        try
        {
            var tokenBytes = Convert.FromBase64String(token);
            var tokenJson = Encoding.UTF8.GetString(tokenBytes);
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            if (tokenData.TryGetProperty("email", out var emailElement) &&
                tokenData.TryGetProperty("sub", out var subElement))
            {
                var email = emailElement.GetString();
                var userId = subElement.GetString();

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userId))
                {
                    userInfo = new CookidooUser
                    {
                        Id = userId,
                        Email = email,
                        // Mock data pro demonstrační účely
                        Name = "Test User",
                        Language = "cs-CZ"
                    };
                    return true;
                }
            }
        }
        catch
        {
            // Ignorujeme chyby při parsování - není to mock token
        }

        return false;
    }

    /// <summary>
    /// Autentizace uživatele pomocí email a hesla podle Python implementace
    /// </summary>
    public async Task<CookidooAuthResponse?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Zahajuji autentizaci uživatele {Email}", email);

            var authRequest = new CookidooAuthRequest
            {
                Username = email,
                Password = password,
                ClientId = _options.ClientId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenPath);
            AddJsonContent(request, authRequest);

            var response = await SendRequestAsync<CookidooAuthResponse>(request, cancellationToken);
            
            if (response != null)
            {
                _logger.LogInformation("Autentizace uživatele {Email} proběhla úspěšně", email);
            }
            
            return response;
        }
        catch (CookidooAuthenticationException)
        {
            _logger.LogWarning("Neplatné přihlašovací údaje pro uživatele {Email}", email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při autentizaci uživatele {Email}", email);
            throw new CookidooAuthenticationException("Chyba při komunikaci s Cookidoo API během autentizace", ex);
        }
    }

    /// <summary>
    /// Obnovení přístupového tokenu pomocí refresh tokenu
    /// </summary>
    public async Task<CookidooAuthResponse?> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obnovuji přístupový token");

            var refreshRequest = new CookidooRefreshRequest
            {
                RefreshToken = refreshToken,
                ClientId = _options.ClientId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _options.RefreshTokenPath);
            AddJsonContent(request, refreshRequest);

            var response = await SendRequestAsync<CookidooAuthResponse>(request, cancellationToken);
            
            if (response != null)
            {
                _logger.LogInformation("Přístupový token byl úspěšně obnoven");
            }
            
            return response;
        }
        catch (CookidooAuthenticationException)
        {
            _logger.LogWarning("Neplatný refresh token");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při obnovování tokenu");
            throw new CookidooAuthenticationException("Chyba při komunikaci s Cookidoo API během obnovování tokenu", ex);
        }
    }

    #region Recepty

    public async Task<Recipe> CreateRecipeAsync(string token, Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Vytváření receptu: {RecipeName}", recipe.Name);

            // Krok 1: Vytvoření základního receptu (podle cookiput projektu)
            var createRequest = CookidooMapper.ToCreateRequest(recipe);
            var request = CreateAuthenticatedRequest(HttpMethod.Post, $"/created-recipes/{_options.DefaultLanguage}", token);
            AddJsonContent(request, createRequest);

            var createResponse = await SendRequestAsync<CreateRecipeResponse>(request, cancellationToken);
            if (createResponse == null)
                throw new CookidooApiException("Nepodařilo se vytvořit recept");

            recipe.Id = createResponse.RecipeId;

            // Krok 2: Aktualizace s detaily receptu
            await UpdateRecipeDetailsAsync(token, recipe.Id, recipe, cancellationToken);

            _logger.LogInformation("Recept úspěšně vytvořen s ID: {RecipeId}", recipe.Id);
            return recipe;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při vytváření receptu: {RecipeName}", recipe.Name);
            throw new CookidooApiException("Nepodařilo se vytvořit recept", ex);
        }
    }

    public async Task<Recipe> UpdateRecipeAsync(string token, string recipeId, Recipe recipe, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Aktualizace receptu: {RecipeId}", recipeId);

            recipe.Id = recipeId;
            recipe.UpdatedAt = DateTime.UtcNow;

            await UpdateRecipeDetailsAsync(token, recipeId, recipe, cancellationToken);

            _logger.LogInformation("Recept úspěšně aktualizován: {RecipeId}", recipeId);
            return recipe;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při aktualizaci receptu: {RecipeId}", recipeId);
            throw new CookidooApiException("Nepodařilo se aktualizovat recept", ex);
        }
    }

    private async Task UpdateRecipeDetailsAsync(string token, string recipeId, Recipe recipe, CancellationToken cancellationToken)
    {
        var updateRequest = CookidooMapper.ToUpdateRequest(recipe);
        var request = CreateAuthenticatedRequest(HttpMethod.Patch, $"/created-recipes/{_options.DefaultLanguage}/{recipeId}", token);
        AddJsonContent(request, updateRequest);

        await SendRequestAsync(request, cancellationToken);
    }

    public async Task<Recipe?> GetRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání receptu: {RecipeId}", recipeId);

            var request = CreateAuthenticatedRequest(HttpMethod.Get, $"/recipes/{recipeId}", token);
            var response = await SendRequestAsync<CookidooRecipeDto>(request, cancellationToken);

            if (response == null) return null;

            var recipe = CookidooMapper.FromRecipeDto(response);
            _logger.LogInformation("Recept úspěšně načten: {RecipeId}", recipeId);
            return recipe;
        }
        catch (CookidooNotFoundException)
        {
            return null;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání receptu: {RecipeId}", recipeId);
            throw new CookidooApiException("Nepodařilo se načíst recept", ex);
        }
    }

    public async Task<List<Recipe>> GetMyRecipesAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání vlastních receptů");

            var request = CreateAuthenticatedRequest(HttpMethod.Get, "/created-recipes", token);
            var response = await SendRequestAsync<CookidooRecipeListResponse>(request, cancellationToken);

            if (response == null) return new List<Recipe>();

            var recipes = response.Recipes.Select(CookidooMapper.FromRecipeDto).ToList();
            _logger.LogInformation("Načteno {Count} vlastních receptů", recipes.Count);
            return recipes;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání vlastních receptů");
            throw new CookidooApiException("Nepodařilo se načíst vlastní recepty", ex);
        }
    }

    public async Task<bool> DeleteRecipeAsync(string token, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Mazání receptu: {RecipeId}", recipeId);

            var request = CreateAuthenticatedRequest(HttpMethod.Delete, $"/created-recipes/{_options.DefaultLanguage}/{recipeId}", token);
            await SendRequestAsync(request, cancellationToken);

            _logger.LogInformation("Recept úspěšně smazán: {RecipeId}", recipeId);
            return true;
        }
        catch (CookidooNotFoundException)
        {
            return false;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při mazání receptu: {RecipeId}", recipeId);
            throw new CookidooApiException("Nepodařilo se smazat recept", ex);
        }
    }

    #endregion

    #region Kolekce

    public async Task<RecipeCollection> CreateCollectionAsync(string token, RecipeCollection collection, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Vytváření kolekce: {CollectionName}", collection.Name);

            var createRequest = CookidooMapper.ToCreateCollectionRequest(collection);
            var request = CreateAuthenticatedRequest(HttpMethod.Post, "/collections", token);
            AddJsonContent(request, createRequest);

            var response = await SendRequestAsync<CookidooCollectionDto>(request, cancellationToken);
            if (response == null)
                throw new CookidooApiException("Nepodařilo se vytvořit kolekci");

            var createdCollection = CookidooMapper.FromCollectionDto(response);
            _logger.LogInformation("Kolekce úspěšně vytvořena s ID: {CollectionId}", createdCollection.Id);
            return createdCollection;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při vytváření kolekce: {CollectionName}", collection.Name);
            throw new CookidooApiException("Nepodařilo se vytvořit kolekci", ex);
        }
    }

    public async Task<RecipeCollection> UpdateCollectionAsync(string token, string collectionId, RecipeCollection collection, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Aktualizace kolekce: {CollectionId}", collectionId);

            var updateRequest = new
            {
                name = collection.Name,
                description = collection.Description,
                tags = collection.Tags,
                isPublic = collection.IsPublic
            };

            var request = CreateAuthenticatedRequest(HttpMethod.Put, $"/collections/{collectionId}", token);
            AddJsonContent(request, updateRequest);

            var response = await SendRequestAsync<CookidooCollectionDto>(request, cancellationToken);
            if (response == null)
                throw new CookidooApiException("Nepodařilo se aktualizovat kolekci");

            var updatedCollection = CookidooMapper.FromCollectionDto(response);
            _logger.LogInformation("Kolekce úspěšně aktualizována: {CollectionId}", collectionId);
            return updatedCollection;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při aktualizaci kolekce: {CollectionId}", collectionId);
            throw new CookidooApiException("Nepodařilo se aktualizovat kolekci", ex);
        }
    }

    public async Task<RecipeCollection?> GetCollectionAsync(string token, string collectionId, bool includeRecipes = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání kolekce: {CollectionId}", collectionId);

            var url = $"/collections/{collectionId}";
            if (includeRecipes)
                url += "?include=recipes";

            var request = CreateAuthenticatedRequest(HttpMethod.Get, url, token);
            var response = await SendRequestAsync<CookidooCollectionDto>(request, cancellationToken);

            if (response == null) return null;

            var collection = CookidooMapper.FromCollectionDto(response);
            _logger.LogInformation("Kolekce úspěšně načtena: {CollectionId}", collectionId);
            return collection;
        }
        catch (CookidooNotFoundException)
        {
            return null;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání kolekce: {CollectionId}", collectionId);
            throw new CookidooApiException("Nepodařilo se načíst kolekci", ex);
        }
    }

    public async Task<List<RecipeCollection>> GetMyCollectionsAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Načítání vlastních kolekcí");

            var request = CreateAuthenticatedRequest(HttpMethod.Get, "/collections/my", token);
            var response = await SendRequestAsync<CookidooCollectionListResponse>(request, cancellationToken);

            if (response == null) return new List<RecipeCollection>();

            var collections = response.Collections.Select(CookidooMapper.FromCollectionDto).ToList();
            _logger.LogInformation("Načteno {Count} vlastních kolekcí", collections.Count);
            return collections;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při načítání vlastních kolekcí");
            throw new CookidooApiException("Nepodařilo se načíst vlastní kolekce", ex);
        }
    }

    public async Task<bool> DeleteCollectionAsync(string token, string collectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Mazání kolekce: {CollectionId}", collectionId);

            var request = CreateAuthenticatedRequest(HttpMethod.Delete, $"/collections/{collectionId}", token);
            await SendRequestAsync(request, cancellationToken);

            _logger.LogInformation("Kolekce úspěšně smazána: {CollectionId}", collectionId);
            return true;
        }
        catch (CookidooNotFoundException)
        {
            return false;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při mazání kolekce: {CollectionId}", collectionId);
            throw new CookidooApiException("Nepodařilo se smazat kolekci", ex);
        }
    }

    public async Task<bool> AddRecipeToCollectionAsync(string token, string collectionId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Přidávání receptu {RecipeId} do kolekce {CollectionId}", recipeId, collectionId);

            var addRequest = CookidooMapper.ToAddRecipeRequest(recipeId);
            var request = CreateAuthenticatedRequest(HttpMethod.Post, $"/collections/{collectionId}/recipes", token);
            AddJsonContent(request, addRequest);

            await SendRequestAsync(request, cancellationToken);

            _logger.LogInformation("Recept úspěšně přidán do kolekce");
            return true;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při přidávání receptu do kolekce");
            throw new CookidooApiException("Nepodařilo se přidat recept do kolekce", ex);
        }
    }

    public async Task<bool> RemoveRecipeFromCollectionAsync(string token, string collectionId, string recipeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Odebírání receptu {RecipeId} z kolekce {CollectionId}", recipeId, collectionId);

            var request = CreateAuthenticatedRequest(HttpMethod.Delete, $"/collections/{collectionId}/recipes/{recipeId}", token);
            await SendRequestAsync(request, cancellationToken);

            _logger.LogInformation("Recept úspěšně odebrán z kolekce");
            return true;
        }
        catch (CookidooNotFoundException)
        {
            return false;
        }
        catch (Exception ex) when (!(ex is CookidooException))
        {
            _logger.LogError(ex, "Chyba při odebírání receptu z kolekce");
            throw new CookidooApiException("Nepodařilo se odebrat recept z kolekce", ex);
        }
    }

    #endregion

    #region HTTP Helper Methods

    private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string endpoint, string token)
    {
        var request = new HttpRequestMessage(method, endpoint);
        
        // Nastavíme stejné headers jako cookiput projekt
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "troet");
        
        // Pro Cookidoo API používáme _oauth2_proxy cookie podle cookiput-main principů
        request.Headers.Add("Cookie", $"_oauth2_proxy={token}");
        
        _logger.LogDebug("Vytvořený autentizovaný požadavek: {Method} {Endpoint}", method, endpoint);
        
        return request;
    }

    private void AddJsonContent<T>(HttpRequestMessage request, T content)
    {
        var json = JsonSerializer.Serialize(content, _jsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Přidáme Content-Type header explicitně pro jistotu
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        if (_options.LogHttpRequests)
        {
            _logger.LogDebug("HTTP Request: {Method} {Uri}\nContent: {Content}",
                request.Method, request.RequestUri, json);
        }
    }

    private async Task<T?> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken) where T : class
    {
        var response = await SendRequestAsync(request, cancellationToken);
        
        if (response == null || string.IsNullOrEmpty(response))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(response, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Chyba při deserializaci odpovědi: {Response}", response);
            throw new CookidooApiException("Neplatná odpověď z Cookidoo API", ex);
        }
    }

    private async Task<string?> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (_options.LogHttpRequests)
            {
                _logger.LogDebug("HTTP Response: {StatusCode}\nContent: {Content}",
                    response.StatusCode, content);
            }

            if (response.IsSuccessStatusCode)
            {
                return content;
            }

            await HandleErrorResponse(response, content);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP chyba při komunikaci s Cookidoo API");
            throw new CookidooApiException("Chyba komunikace s Cookidoo API", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout při komunikaci s Cookidoo API");
            throw new CookidooApiException("Timeout při komunikaci s Cookidoo API", ex);
        }
        finally
        {
            response?.Dispose();
        }
    }

    private Task HandleErrorResponse(HttpResponseMessage response, string content)
    {
        var statusCode = (int)response.StatusCode;

        try
        {
            var errorResponse = JsonSerializer.Deserialize<CookidooErrorResponse>(content, _jsonOptions);
            var errorMessage = errorResponse?.Message ?? errorResponse?.Error ?? "Neznámá chyba";

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new CookidooAuthenticationException($"Autentizace selhala: {errorMessage}");
                
                case System.Net.HttpStatusCode.Forbidden:
                    throw new CookidooAuthorizationException($"Nedostatečná oprávnění: {errorMessage}");
                
                case System.Net.HttpStatusCode.NotFound:
                    throw new CookidooNotFoundException($"Zdroj nebyl nalezen: {errorMessage}");
                
                default:
                    throw new CookidooApiException($"API chyba ({statusCode}): {errorMessage}", statusCode, content);
            }
        }
        catch (JsonException)
        {
            // Pokud se nepodaří deserializovat chybovou odpověď, použijeme obecnou chybu
            throw new CookidooApiException($"API chyba ({statusCode}): {content}", statusCode, content);
        }
    }

    #endregion
} 