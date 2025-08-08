namespace Cookidoo.MCP.Infrastructure.Configuration;

/// <summary>
/// Konfigurační možnosti pro Cookidoo API podle cookidoo-api-master implementace
/// </summary>
public class CookidooOptions
{
    /// <summary>
    /// Název sekce v konfiguraci
    /// </summary>
    public const string SectionName = "Cookidoo";

    /// <summary>
    /// Základní URL pro Cookidoo API podle cookiput projektu
    /// </summary>
    public string BaseUrl { get; set; } = "https://cookidoo.de";

    /// <summary>
    /// Výchozí jazyk pro API volání (např. "de-CH", "en-US")
    /// </summary>
    public string DefaultLanguage { get; set; } = "de-CH";

    /// <summary>
    /// Výchozí country code
    /// </summary>
    public string DefaultCountryCode { get; set; } = "ch";

    /// <summary>
    /// Timeout pro HTTP požadavky v sekundách
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximální počet pokusů při chybě
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Zpožděný mezi pokusy v milisekundách
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// User-Agent string pro HTTP požadavky
    /// </summary>
    public string UserAgent { get; set; } = "Cookidoo-MCP-Server/1.0";

    /// <summary>
    /// Zda logovat HTTP požadavky a odpovědi
    /// </summary>
    public bool LogHttpRequests { get; set; } = false;

    // Autentizační konstanty podle cookidoo-api-master implementace
    /// <summary>
    /// Client ID pro Cookidoo OAuth podle cookidoo-api-master
    /// </summary>
    public string ClientId { get; set; } = "kupferwerk-client-nwot";

    /// <summary>
    /// Client Secret pro Cookidoo OAuth podle cookidoo-api-master
    /// </summary>
    public string ClientSecret { get; set; } = "Ls50ON1woySqs1dCdJge";

    /// <summary>
    /// Authorization header value podle cookidoo-api-master
    /// </summary>
    public string AuthorizationHeader { get; set; } = "Basic a3VwZmVyd2Vyay1jbGllbnQtbndvdDpMczUwT04xd285U3FzMWRDZEpnZQ==";

    /// <summary>
    /// Endpoint pro získání access tokenu podle cookidoo-api-master
    /// </summary>
    public string TokenPath { get; set; } = "ciam/auth/token";

    /// <summary>
    /// Endpoint pro refresh token
    /// </summary>
    public string RefreshTokenPath { get; set; } = "ciam/auth/token";

    /// <summary>
    /// Endpoint pro ověření uživatele
    /// </summary>
    public string UserPath { get; set; } = "community/profile";

    /// <summary>
    /// API endpoint pattern podle cookidoo-api-master
    /// </summary>
    public string ApiEndpointPattern { get; set; } = "https://{0}.tmmobile.vorwerk-digital.com";

    /// <summary>
    /// International country code
    /// </summary>
    public string InternationalCountryCode { get; set; } = "xp";

    /// <summary>
    /// UK country code
    /// </summary>
    public string UkCountryCode { get; set; } = "gb";
} 