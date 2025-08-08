using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cookidoo.MCP.Infrastructure.Configuration;
using Cookidoo.MCP.Infrastructure.Models;
using Cookidoo.MCP.Core.Exceptions;
using System.Net;
using System.Net.Http.Headers;

namespace Cookidoo.MCP.Infrastructure.Services;

/// <summary>
/// Služba pro autentizaci s Cookidoo API podle cookidoo-api-master implementace
/// </summary>
public class CookidooAuthService
{
    private readonly ILogger<CookidooAuthService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CookidooOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public CookidooAuthService(
        ILogger<CookidooAuthService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<CookidooOptions> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Přihlásí uživatele pomocí OAuth2 API podle cookidoo-api-master implementace
    /// </summary>
    /// <param name="email">Email uživatele</param>
    /// <param name="password">Heslo uživatele</param>
    /// <returns>Autentizační odpověď s access tokenem</returns>
    /// <exception cref="CookidooAuthenticationException">Při chybě autentizace</exception>
    public async Task<CookidooAuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Zahajuji OAuth2 přihlášení uživatele {Email} podle cookidoo-api-master", email);

            // Validace vstupních dat
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new CookidooAuthenticationException("Email a heslo jsou povinné");
            }

            using var httpClient = CreateHttpClient();

            // Příprava OAuth2 form data podle cookidoo-api-master
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("username", email),
                new("password", password),
                new("client_id", _options.ClientId)
            };

            var tokenUrl = GetApiEndpoint() + "/" + _options.TokenPath;
            _logger.LogDebug("Odesílám OAuth2 request na: {TokenUrl}", tokenUrl);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Content = new FormUrlEncodedContent(formData);
            
            // Nastavení headerů podle cookidoo-api-master
            request.Headers.Add("Accept", "application/json");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", 
                _options.AuthorizationHeader.Replace("Basic ", ""));

            var response = await httpClient.SendAsync(request);

            _logger.LogDebug("OAuth2 response status: {StatusCode}", response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("OAuth2 autentizace selhala pro {Email}: {Error}", email, errorContent);
                throw new CookidooAuthenticationException("Neplatné přihlašovací údaje");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("OAuth2 bad request pro {Email}: {Error}", email, errorContent);
                throw new CookidooAuthenticationException("Neplatný požadavek na autentizaci");
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<CookidooAuthResponse>(responseContent, _jsonOptions);

            if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
            {
                throw new CookidooAuthenticationException("Neplatná odpověď z Cookidoo API");
            }

            _logger.LogInformation("OAuth2 přihlášení uživatele {Email} bylo úspěšné", email);
            return authResponse;
        }
        catch (CookidooAuthenticationException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Síťová chyba při OAuth2 přihlašování uživatele {Email}", email);
            throw new CookidooAuthenticationException("Cookidoo API není dostupné", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Chyba při parsování OAuth2 odpovědi pro uživatele {Email}", email);
            throw new CookidooAuthenticationException("Neplatná odpověď z Cookidoo API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při OAuth2 přihlašování uživatele {Email}", email);
            throw new CookidooAuthenticationException("Došlo k neočekávané chybě při komunikaci s Cookidoo", ex);
        }
    }

    /// <summary>
    /// Obnoví access token pomocí refresh tokenu podle cookidoo-api-master
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>Nová autentizační odpověď</returns>
    /// <exception cref="CookidooAuthenticationException">Při chybě obnovení tokenu</exception>
    public async Task<CookidooAuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogDebug("Zahajuji obnovení OAuth2 tokenu podle cookidoo-api-master");

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new CookidooAuthenticationException("Refresh token je povinný");
            }

            using var httpClient = CreateHttpClient();

            // Příprava refresh token form data podle cookidoo-api-master
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "refresh_token"),
                new("refresh_token", refreshToken),
                new("client_id", _options.ClientId)
            };

            var tokenUrl = GetApiEndpoint() + "/" + _options.RefreshTokenPath;
            _logger.LogDebug("Odesílám refresh token request na: {TokenUrl}", tokenUrl);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Content = new FormUrlEncodedContent(formData);
            
            // Nastavení headerů podle cookidoo-api-master
            request.Headers.Add("Accept", "application/json");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", 
                _options.AuthorizationHeader.Replace("Basic ", ""));

            var response = await httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Refresh token je neplatný: {Error}", errorContent);
                throw new CookidooAuthenticationException("Neplatný refresh token");
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<CookidooAuthResponse>(responseContent, _jsonOptions);

            if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
            {
                throw new CookidooAuthenticationException("Neplatná odpověď při obnovení tokenu");
            }

            _logger.LogInformation("OAuth2 token byl úspěšně obnoven");
            return authResponse;
        }
        catch (CookidooAuthenticationException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Síťová chyba při obnovování OAuth2 tokenu");
            throw new CookidooAuthenticationException("Cookidoo API není dostupné", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Chyba při parsování refresh token odpovědi");
            throw new CookidooAuthenticationException("Neplatná odpověď z Cookidoo API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při obnovování OAuth2 tokenu");
            throw new CookidooAuthenticationException("Došlo k neočekávané chybě při komunikaci s Cookidoo", ex);
        }
    }

    /// <summary>
    /// Získá informace o uživateli podle cookidoo-api-master
    /// </summary>
    /// <param name="accessToken">Access token</param>
    /// <returns>Informace o uživateli</returns>
    public async Task<CookidooUserInfo> GetUserInfoAsync(string accessToken)
    {
        try
        {
            _logger.LogDebug("Načítám informace o uživateli podle cookidoo-api-master");

            using var httpClient = CreateHttpClient();

            var userUrl = GetApiEndpoint() + "/" + _options.UserPath;
            _logger.LogDebug("Odesílám user info request na: {UserUrl}", userUrl);

            using var request = new HttpRequestMessage(HttpMethod.Get, userUrl);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new CookidooAuthenticationException("Access token je neplatný nebo expiroval");
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var profileResponse = JsonSerializer.Deserialize<CookidooProfileResponse>(responseContent, _jsonOptions);

            if (profileResponse?.UserInfo == null)
            {
                throw new CookidooAuthenticationException("Neplatná odpověď při načítání informací o uživateli");
            }

            _logger.LogInformation("Informace o uživateli byly úspěšně načteny");
            return profileResponse.UserInfo;
        }
        catch (CookidooAuthenticationException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Síťová chyba při načítání informací o uživateli");
            throw new CookidooAuthenticationException("Cookidoo API není dostupné", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Chyba při parsování user info odpovědi");
            throw new CookidooAuthenticationException("Neplatná odpověď z Cookidoo API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při načítání informací o uživateli");
            throw new CookidooAuthenticationException("Došlo k neočekávané chybě při komunikaci s Cookidoo", ex);
        }
    }

    /// <summary>
    /// Ověří platnost access tokenu
    /// </summary>
    /// <param name="accessToken">Access token k ověření</param>
    /// <returns>True pokud je token platný</returns>
    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            await GetUserInfoAsync(accessToken);
            return true;
        }
        catch (CookidooAuthenticationException)
        {
            return false;
        }
    }

    /// <summary>
    /// Vytvoří HTTP klient s výchozími nastaveními podle cookidoo-api-master
    /// </summary>
    private HttpClient CreateHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        httpClient.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);
        return httpClient;
    }

    /// <summary>
    /// Získá API endpoint podle lokalizace podle cookidoo-api-master
    /// </summary>
    private string GetApiEndpoint()
    {
        var countryCode = _options.DefaultCountryCode;
        
        // Speciální případy podle cookidoo-api-master
        if (countryCode == "international")
        {
            countryCode = _options.InternationalCountryCode;
        }
        else if (countryCode == "co.uk")
        {
            countryCode = _options.UkCountryCode;
        }

        return string.Format(_options.ApiEndpointPattern, countryCode);
    }

    /// <summary>
    /// Simulované odhlášení (Cookidoo API nemusí mít explicitní logout endpoint)
    /// </summary>
    /// <param name="accessToken">Access token k invalidaci</param>
    /// <returns>True pokud bylo odhlášení úspěšné</returns>
    public async Task<bool> LogoutAsync(string accessToken)
    {
        // Cookidoo API nemusí mít explicitní logout endpoint
        // V reálné implementaci by se token mohl přidat do blacklistu
        _logger.LogInformation("Simuluji odhlášení z Cookidoo API");
        await Task.Delay(100); // Simulace síťového volání
        return true;
    }
} 